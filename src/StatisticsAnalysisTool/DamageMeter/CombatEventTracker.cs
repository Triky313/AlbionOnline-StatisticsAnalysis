using Serilog;
using StatisticsAnalysisTool.Cluster;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.GameFileData;
using StatisticsAnalysisTool.GameFileData.Models;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Models.NetworkModel;
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
    private static readonly TimeSpan RecentlyLeftMobRetention = TimeSpan.FromSeconds(10);
    private readonly Lock _syncLock = new();
    private readonly ConcurrentDictionary<long, CombatMobCacheEntry> _knownMobs = new();
    private readonly ConcurrentDictionary<long, CombatMobCacheEntry> _recentlyLeftMobs = new();
    private readonly List<CombatEvent> _combatEvents = [];
    private readonly Dictionary<Guid, Dictionary<DashboardContentType, CombatMobDamageStats>> _mobDamageStatsByInstance = [];
    private readonly Dictionary<DashboardContentType, long> _confirmedMobDamageByContent = [];
    private long _confirmedMobDamage;
    private long _mobDamageStatsVersion;
    private readonly HashSet<long> _partyPlayersInCombat = [];
    private readonly MobDataResolver _mobDataResolver = new();
    private CombatEvent _activeCombatEvent;

    public IReadOnlyCollection<CombatMobCacheEntry> KnownMobs => _knownMobs.Values.ToList();

    public CombatMobCacheEntry GetKnownMobOrDefault(long objectId)
    {
        if (_knownMobs.TryGetValue(objectId, out var knownMob))
        {
            return knownMob;
        }

        if (_recentlyLeftMobs.TryGetValue(objectId, out var recentlyLeftMob) && IsRecentlyLeftMobValid(recentlyLeftMob))
        {
            return recentlyLeftMob;
        }

        _recentlyLeftMobs.TryRemove(objectId, out _);
        return null;
    }

    public void RemoveKnownMob(long objectId)
    {
        if (!_knownMobs.TryRemove(objectId, out var removedMob))
        {
            return;
        }

        removedMob.LastUpdated = DateTime.UtcNow;
        _recentlyLeftMobs[objectId] = removedMob;
        RemoveExpiredRecentlyLeftMobs();
    }

    public IReadOnlyCollection<CombatEvent> CombatEvents => GetCombatEvents(null);

    public IReadOnlyCollection<CombatEvent> GetCombatEvents(DashboardContentType? contentType)
    {
        lock (_syncLock)
        {
            return _combatEvents
                .Where(x => !contentType.HasValue || x.ContentType == contentType.Value)
                .Select(x => x.Clone())
                .ToList();
        }
    }

    public IReadOnlyCollection<CombatMobDamageStats> GetMobDamageStats(DashboardContentType? contentType = null)
    {
        lock (_syncLock)
        {
            return _mobDamageStatsByInstance.Values
                .SelectMany(x => x.Values)
                .Where(x => x.IsConfirmedMob)
                .Where(x => !contentType.HasValue || x.ContentType == contentType.Value)
                .Select(x => x.Clone())
                .ToList();
        }
    }

    public CombatMobDamageStatsUpdate GetMobDamageStatsUpdate(DashboardContentType? contentType, long afterVersion)
    {
        lock (_syncLock)
        {
            var effectiveAfterVersion = afterVersion <= _mobDamageStatsVersion ? afterVersion : 0;
            var changedMobs = _mobDamageStatsByInstance.Values
                .SelectMany(x => x.Values)
                .Where(x => x.IsConfirmedMob)
                .Where(x => !contentType.HasValue || x.ContentType == contentType.Value)
                .Where(x => x.Version > effectiveAfterVersion)
                .Select(x => x.Clone())
                .ToList();

            return new CombatMobDamageStatsUpdate
            {
                Version = _mobDamageStatsVersion,
                TotalDamage = contentType.HasValue
                    ? _confirmedMobDamageByContent.GetValueOrDefault(contentType.Value)
                    : _confirmedMobDamage,
                ChangedMobs = changedMobs
            };
        }
    }

    public bool TryGetMobInstanceId(long objectId, out Guid mobInstanceId)
    {
        var mob = GetKnownMobOrDefault(objectId);
        if (mob == null)
        {
            mobInstanceId = Guid.Empty;
            return false;
        }

        mobInstanceId = mob.MobInstanceId;
        return true;
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
        var shouldLogUnknownMobData = false;

        _recentlyLeftMobs.TryRemove(mobObjectId, out _);
        var trackedMob = _knownMobs.AddOrUpdate(
            mobObjectId,
            _ =>
            {
                shouldLogUnknownMobData = true;
                return CreateMobCacheEntry(newMobEvent, mobObjectId, clusterKey, now, mobData);
            },
            (_, existingEntry) =>
            {
                if (!string.Equals(existingEntry.ClusterKey, clusterKey, StringComparison.Ordinal))
                {
                    shouldLogUnknownMobData = true;
                    return CreateMobCacheEntry(newMobEvent, mobObjectId, clusterKey, now, mobData);
                }

                shouldLogUnknownMobData = existingEntry.IsProvisional;
                existingEntry.MobIndex = newMobEvent.MobIndex;
                existingEntry.Health = newMobEvent.HitPoints;
                existingEntry.MaxHealth = newMobEvent.HitPointsMax;
                existingEntry.MapTier = GetCurrentMapTier();
                existingEntry.LastUpdated = now;
                existingEntry.MobData = mobData;
                existingEntry.UniqueName = mobData.UniqueName;
                existingEntry.TypeId = newMobEvent.MobIndex.ToString();
                existingEntry.IsProvisional = false;
                return existingEntry;
            });

        UpdateMobDamageStats(trackedMob);

        if (shouldLogUnknownMobData && string.Equals(mobData.UniqueName, "UNKNOWN_MOB", StringComparison.Ordinal))
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
                EnsureActiveCombatEvent(false, ResolveCurrentContentType());
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

    public void AddHealthContribution(CombatEventValueType valueType, long sourceObjectId, long targetObjectId, long value, int causingSpellIndex, DashboardContentType contentType)
    {
        if (value <= 0)
        {
            return;
        }

        lock (_syncLock)
        {
            EnsureActiveCombatEvent(true, contentType);

            if (_activeCombatEvent == null)
            {
                return;
            }

            var sourcePlayer = GetPlayer(sourceObjectId);
            var targetPlayer = GetPlayer(targetObjectId);
            var sourceMob = sourcePlayer == null ? GetKnownMob(sourceObjectId) : null;
            var targetMob = targetPlayer == null ? GetKnownMob(targetObjectId) : null;

            if (valueType == CombatEventValueType.Damage
                && sourcePlayer != null
                && targetPlayer == null
                && targetMob == null)
            {
                targetMob = GetOrCreateProvisionalMob(targetObjectId);
            }

            AddKnownParticipant(sourceObjectId, sourcePlayer, sourceMob);
            AddKnownParticipant(targetObjectId, targetPlayer, targetMob);

            var participant = GetAggregationParticipant(valueType, sourceObjectId, targetObjectId, sourcePlayer, targetPlayer, sourceMob, targetMob);
            _activeCombatEvent.AddContribution(valueType, sourceObjectId, targetObjectId, targetMob?.MobInstanceId, value, causingSpellIndex, participant);

            if (valueType == CombatEventValueType.Damage && targetMob != null)
            {
                RecordMobDamage(targetMob, contentType, sourcePlayer, causingSpellIndex, value);
            }
        }
    }

    public void OnClusterChanged()
    {
        lock (_syncLock)
        {
            EndActiveCombatEvent();
            _partyPlayersInCombat.Clear();
            _knownMobs.Clear();
            _recentlyLeftMobs.Clear();
        }
    }

    public void ClearCombatEvents()
    {
        lock (_syncLock)
        {
            EndActiveCombatEvent();
            _combatEvents.Clear();
            _partyPlayersInCombat.Clear();
            _mobDamageStatsByInstance.Clear();
            _confirmedMobDamageByContent.Clear();
            _recentlyLeftMobs.Clear();
            _confirmedMobDamage = 0;
            _mobDamageStatsVersion++;
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
            Health = newMobEvent.HitPoints,
            MaxHealth = newMobEvent.HitPointsMax,
            MapTier = GetCurrentMapTier(),
            FirstSeen = now,
            LastUpdated = now,
            MobData = mobData,
            IsProvisional = false
        };
    }

    private CombatMobCacheEntry GetOrCreateProvisionalMob(long mobObjectId)
    {
        var now = DateTime.UtcNow;
        var clusterKey = GetCurrentClusterKey();

        CombatMobCacheEntry CreateEntry()
        {
            return new CombatMobCacheEntry
            {
                ClusterKey = clusterKey,
                ClusterName = GetCurrentClusterName(),
                MapTier = GetCurrentMapTier(),
                MobObjectId = mobObjectId,
                MobIndex = 0,
                UniqueName = "UNKNOWN_MOB",
                TypeId = "0",
                Health = 0,
                MaxHealth = 0,
                FirstSeen = now,
                LastUpdated = now,
                MobData = null,
                IsProvisional = true
            };
        }

        return _knownMobs.AddOrUpdate(
            mobObjectId,
            _ => CreateEntry(),
            (_, existingEntry) => string.Equals(existingEntry.ClusterKey, clusterKey, StringComparison.Ordinal) ? existingEntry : CreateEntry());
    }

    private void EnsureActiveCombatEvent(bool isImplicit, DashboardContentType contentType)
    {
        var clusterKey = GetCurrentClusterKey();
        if (_activeCombatEvent?.IsActive == true
            && _activeCombatEvent.ClusterKey == clusterKey
            && _activeCombatEvent.ContentType == contentType)
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

        if (_activeCombatEvent?.IsActive == true
            && _activeCombatEvent.ClusterKey == clusterKey
            && _activeCombatEvent.ContentType == contentType)
        {
            return;
        }

        EndActiveCombatEvent();

        _activeCombatEvent = new CombatEvent
        {
            ClusterKey = clusterKey,
            ClusterName = GetCurrentClusterName(),
            ContentType = contentType,
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

    private void AddKnownParticipant(long objectId, PlayerGameObject player, CombatMobCacheEntry mob)
    {
        if (player != null)
        {
            _activeCombatEvent?.AddPlayerObjectId(objectId);
            return;
        }

        if (mob != null)
        {
            _activeCombatEvent?.AddMob(mob);
        }
    }

    private static CombatEventParticipant GetAggregationParticipant(
        CombatEventValueType valueType,
        long sourceObjectId,
        long targetObjectId,
        PlayerGameObject sourcePlayer,
        PlayerGameObject targetPlayer,
        CombatMobCacheEntry sourceMob,
        CombatMobCacheEntry targetMob)
    {
        var participantObjectId = valueType switch
        {
            CombatEventValueType.TakenDamage => targetObjectId,
            _ => sourceObjectId
        };

        var player = valueType == CombatEventValueType.TakenDamage ? targetPlayer : sourcePlayer;
        if (player != null)
        {
            return new CombatEventParticipant
            {
                ObjectId = participantObjectId,
                Name = player.Name,
                IsPlayer = true
            };
        }

        var mob = valueType == CombatEventValueType.TakenDamage ? targetMob : sourceMob;
        if (mob != null)
        {
            return new CombatEventParticipant
            {
                ObjectId = participantObjectId,
                Name = GetMobDisplayName(mob),
                IsMob = true
            };
        }

        return null;
    }

    private PlayerGameObject GetPlayer(long objectId)
    {
        var entity = trackingController.EntityController.GetEntity(objectId);
        return entity?.Value is { ObjectType: GameObjectType.Player } player ? player : null;
    }

    private void RecordMobDamage(CombatMobCacheEntry mob, DashboardContentType contentType, PlayerGameObject sourcePlayer, int causingSpellIndex, long value)
    {
        if (!_mobDamageStatsByInstance.TryGetValue(mob.MobInstanceId, out var statsByContent))
        {
            statsByContent = [];
            _mobDamageStatsByInstance.Add(mob.MobInstanceId, statsByContent);
        }

        if (!statsByContent.TryGetValue(contentType, out var mobDamageStats))
        {
            mobDamageStats = new CombatMobDamageStats
            {
                MobInstanceId = mob.MobInstanceId,
                MobObjectId = mob.MobObjectId,
                ClusterKey = mob.ClusterKey,
                ClusterName = mob.ClusterName,
                ContentType = contentType,
                FirstSeen = mob.FirstSeen
            };
            _ = mobDamageStats.UpdateMob(mob);
            statsByContent.Add(contentType, mobDamageStats);
        }

        var presentationSpellIndex = SpellPresentationResolver.ResolveSpellIndex(
            causingSpellIndex,
            sourcePlayer?.CharacterEquipment?.ActiveSpells?.Select(spell => spell.Value));
        mobDamageStats.RecordDamage(
            sourcePlayer?.UserGuid,
            sourcePlayer?.Name ?? string.Empty,
            presentationSpellIndex,
            sourcePlayer?.LastContributionWeaponItemIndex ?? 0,
            value,
            DateTime.UtcNow);
        mobDamageStats.MarkUpdated(++_mobDamageStatsVersion);

        if (mobDamageStats.IsConfirmedMob)
        {
            _confirmedMobDamage += value;
            _confirmedMobDamageByContent[contentType] = _confirmedMobDamageByContent.GetValueOrDefault(contentType) + value;
        }
    }

    private void UpdateMobDamageStats(CombatMobCacheEntry mob)
    {
        lock (_syncLock)
        {
            if (_mobDamageStatsByInstance.TryGetValue(mob.MobInstanceId, out var statsByContent))
            {
                foreach (var mobDamageStats in statsByContent.Values)
                {
                    var becameConfirmedMob = mobDamageStats.UpdateMob(mob);
                    mobDamageStats.MarkUpdated(++_mobDamageStatsVersion);

                    if (becameConfirmedMob)
                    {
                        _confirmedMobDamage += mobDamageStats.Damage;
                        _confirmedMobDamageByContent[mobDamageStats.ContentType] =
                            _confirmedMobDamageByContent.GetValueOrDefault(mobDamageStats.ContentType) + mobDamageStats.Damage;
                    }
                }
            }

            if (_activeCombatEvent?.MobInstanceIds.Contains(mob.MobInstanceId) == true)
            {
                _activeCombatEvent.AddMob(mob);
            }
        }
    }

    private static string GetMobDisplayName(CombatMobCacheEntry mob)
    {
        if (mob.MobData != null)
        {
            var localizedName = MobsData.GetLocalizedMobName(mob.MobData);
            if (!string.IsNullOrWhiteSpace(localizedName))
            {
                return localizedName;
            }
        }

        return !string.IsNullOrWhiteSpace(mob.UniqueName)
            ? mob.UniqueName
            : mob.MobObjectId.ToString();
    }

    private CombatMobCacheEntry GetKnownMob(long objectId)
    {
        return GetKnownMobOrDefault(objectId);
    }

    private static bool IsRecentlyLeftMobValid(CombatMobCacheEntry mob)
    {
        return DateTime.UtcNow - mob.LastUpdated <= RecentlyLeftMobRetention;
    }

    private void RemoveExpiredRecentlyLeftMobs()
    {
        foreach (var mob in _recentlyLeftMobs.ToArray())
        {
            if (!IsRecentlyLeftMobValid(mob.Value))
            {
                _recentlyLeftMobs.TryRemove(mob.Key, out _);
            }
        }
    }

    private DashboardContentType ResolveCurrentContentType()
    {
        var currentCluster = ClusterController.CurrentCluster;
        return DashboardContentTypeResolver.Resolve(
            currentCluster.MapType,
            trackingController.StatisticController.ResolveDungeonMode(currentCluster.MapType),
            currentCluster.ClusterMode);
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

    private static Tier GetCurrentMapTier()
    {
        var currentCluster = ClusterController.CurrentCluster;
        return currentCluster.MapType switch
        {
            MapType.RandomDungeon when currentCluster.RandomDungeonTier != Tier.Unknown => currentCluster.RandomDungeonTier,
            MapType.MistsDungeon when currentCluster.MistsDungeonTier != Tier.Unknown => currentCluster.MistsDungeonTier,
            _ => currentCluster.Tier
        };
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
