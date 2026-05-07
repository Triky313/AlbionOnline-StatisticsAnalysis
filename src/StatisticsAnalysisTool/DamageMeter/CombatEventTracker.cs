using Serilog;
using StatisticsAnalysisTool.Cluster;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.GameFileData.Models;
using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace StatisticsAnalysisTool.DamageMeter;

public sealed class CombatEventTracker(TrackingController trackingController)
{
    private static readonly TimeSpan ImplicitCombatEventTimeout = TimeSpan.FromSeconds(10);
    private readonly Lock _syncLock = new();
    private readonly ConcurrentDictionary<CombatMobKey, CombatMobCacheEntry> _knownMobs = new();
    private readonly List<CombatEvent> _combatEvents = [];
    private readonly HashSet<long> _partyPlayersInCombat = [];
    private readonly MobDataResolver _mobDataResolver = new();
    private CombatEvent _activeCombatEvent;

    public IReadOnlyCollection<CombatMobCacheEntry> KnownMobs => _knownMobs.Values.ToList();
    public IReadOnlyCollection<CombatEvent> CombatEvents
    {
        get
        {
            lock (_syncLock)
            {
                return _combatEvents.ToList();
            }
        }
    }

    public void TrackNewMob(NewMobEvent newMobEvent)
    {
        if (newMobEvent?.ObjectId is not { } mobObjectId)
        {
            return;
        }

        var now = DateTime.UtcNow;
        var clusterKey = GetCurrentClusterKey();
        var mobData = _mobDataResolver.Resolve(newMobEvent);
        var key = new CombatMobKey(clusterKey, mobObjectId);
        var isNewMobEntry = false;

        _knownMobs.AddOrUpdate(
            key,
            _ =>
            {
                isNewMobEntry = true;
                return CreateMobCacheEntry(newMobEvent, mobObjectId, clusterKey, now, mobData);
            },
            (_, existingEntry) =>
            {
                existingEntry.MobIndex = newMobEvent.MobIndex;
                existingEntry.Health = newMobEvent.HitPoints;
                existingEntry.MaxHealth = newMobEvent.HitPointsMax;
                existingEntry.LastUpdated = now;
                existingEntry.MobData = mobData;
                existingEntry.UniqueName = mobData.UniqueName;
                existingEntry.TypeId = newMobEvent.MobIndex.ToString();
                existingEntry.Identifier = mobData.UniqueName;
                return existingEntry;
            });

        if (isNewMobEntry && string.Equals(mobData.UniqueName, "UNKNOWN_MOB", StringComparison.Ordinal))
        {
            Log.Debug("Unknown mob data for NewMob event | MobIndex={MobIndex} | ObjectId={ObjectId} | Cluster={Cluster}", newMobEvent.MobIndex, mobObjectId, clusterKey);
        }
    }

    public void OnCombatStateUpdate(long objectId, bool inActiveCombat, bool inPassiveCombat)
    {
        if (!trackingController.EntityController.IsEntityInParty(objectId))
        {
            return;
        }

        lock (_syncLock)
        {
            var isInCombat = inActiveCombat || inPassiveCombat;
            if (isInCombat)
            {
                _partyPlayersInCombat.Add(objectId);
                EnsureActiveCombatEvent(false);
                _activeCombatEvent?.AddPlayerObjectId(objectId);
                return;
            }

            _partyPlayersInCombat.Remove(objectId);
            if (_partyPlayersInCombat.Count == 0)
            {
                EndActiveCombatEvent();
            }
        }
    }

    public void AddHealthContribution(CombatEventValueType valueType, long sourceObjectId, long targetObjectId, long value, int causingSpellIndex)
    {
        if (value <= 0)
        {
            return;
        }

        lock (_syncLock)
        {
            EnsureActiveCombatEvent(true);

            if (_activeCombatEvent == null)
            {
                return;
            }

            AddKnownParticipant(sourceObjectId);
            AddKnownParticipant(targetObjectId);

            var participant = GetAggregationParticipant(valueType, sourceObjectId, targetObjectId);
            _activeCombatEvent.AddContribution(valueType, sourceObjectId, targetObjectId, value, causingSpellIndex, participant);
        }
    }

    public void OnClusterChanged()
    {
        lock (_syncLock)
        {
            EndActiveCombatEvent();
            _partyPlayersInCombat.Clear();
            _knownMobs.Clear();
        }
    }

    private CombatMobCacheEntry CreateMobCacheEntry(NewMobEvent newMobEvent, long mobObjectId, string clusterKey, DateTime now, MobJsonObject mobData)
    {
        return new CombatMobCacheEntry
        {
            ClusterKey = clusterKey,
            ClusterName = GetCurrentClusterName(),
            MobObjectId = mobObjectId,
            MobIndex = newMobEvent.MobIndex,
            UniqueName = mobData.UniqueName,
            TypeId = newMobEvent.MobIndex.ToString(),
            Identifier = mobData.UniqueName,
            Health = newMobEvent.HitPoints,
            MaxHealth = newMobEvent.HitPointsMax,
            FirstSeen = now,
            LastUpdated = now,
            MobData = mobData
        };
    }

    private void EnsureActiveCombatEvent(bool isImplicit)
    {
        var clusterKey = GetCurrentClusterKey();
        if (_activeCombatEvent?.IsActive == true && _activeCombatEvent.ClusterKey == clusterKey)
        {
            if (_activeCombatEvent.IsImplicit && DateTime.UtcNow - _activeCombatEvent.LastEventTime > ImplicitCombatEventTimeout)
            {
                EndActiveCombatEvent();
            }
            else
            {
                if (!isImplicit)
                {
                    _activeCombatEvent.MarkExplicit();
                }

                return;
            }
        }

        if (_activeCombatEvent?.IsActive == true && _activeCombatEvent.ClusterKey == clusterKey)
        {
            return;
        }

        EndActiveCombatEvent();

        _activeCombatEvent = new CombatEvent
        {
            ClusterKey = clusterKey,
            ClusterName = GetCurrentClusterName(),
            StartTime = DateTime.UtcNow
        };

        if (isImplicit)
        {
            _activeCombatEvent.MarkImplicit();
        }

        _combatEvents.Add(_activeCombatEvent);
    }

    private void EndActiveCombatEvent()
    {
        if (_activeCombatEvent == null)
        {
            return;
        }

        _activeCombatEvent.End(DateTime.UtcNow);
        _activeCombatEvent = null;
    }

    private void AddKnownParticipant(long objectId)
    {
        var player = trackingController.EntityController.GetEntity(objectId);
        if (player?.Value is { ObjectType: GameObjectType.Player })
        {
            _activeCombatEvent?.AddPlayerObjectId(objectId);
            return;
        }

        var mob = GetKnownMob(objectId);
        if (mob != null)
        {
            _activeCombatEvent?.AddMob(mob);
        }
    }

    private CombatEventParticipant GetAggregationParticipant(CombatEventValueType valueType, long sourceObjectId, long targetObjectId)
    {
        var participantObjectId = valueType switch
        {
            CombatEventValueType.TakenDamage => targetObjectId,
            _ => sourceObjectId
        };

        var player = trackingController.EntityController.GetEntity(participantObjectId);
        if (player?.Value is { ObjectType: GameObjectType.Player } playerObject)
        {
            return new CombatEventParticipant
            {
                ObjectId = participantObjectId,
                Name = playerObject.Name,
                IsPlayer = true
            };
        }

        var mob = GetKnownMob(participantObjectId);
        if (mob != null)
        {
            return new CombatEventParticipant
            {
                ObjectId = participantObjectId,
                Name = mob.MobName ?? mob.UniqueName,
                IsMob = true
            };
        }

        return null;
    }

    private CombatMobCacheEntry GetKnownMob(long objectId)
    {
        var key = new CombatMobKey(GetCurrentClusterKey(), objectId);
        return _knownMobs.GetValueOrDefault(key);
    }

    private static string GetCurrentClusterKey()
    {
        var currentCluster = ClusterController.CurrentCluster;
        if (currentCluster.Guid is { } clusterGuid)
        {
            return clusterGuid.ToString("D");
        }

        return $"{currentCluster.MapType}|{currentCluster.Index}|{currentCluster.InstanceName}|{currentCluster.SourceClusterIndex}";
    }

    private static string GetCurrentClusterName()
    {
        var currentCluster = ClusterController.CurrentCluster;
        if (!string.IsNullOrWhiteSpace(currentCluster.MapHistoryClipboardName))
        {
            return currentCluster.MapHistoryClipboardName;
        }

        return currentCluster.Index ?? string.Empty;
    }
}