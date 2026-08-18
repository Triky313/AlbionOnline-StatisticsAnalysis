using Serilog;
using StatisticsAnalysisTool.DamageMeter;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace StatisticsAnalysisTool.Network.Manager;

public class MobKillController(TrackingController trackingController)
{
    private const string UnknownMobUniqueName = "UNKNOWN_MOB";
    private static readonly TimeSpan PendingMobKillRetention = TimeSpan.FromSeconds(2);

    private readonly ConcurrentDictionary<long, byte> _recordedKilledMobs = new();
    private readonly ConcurrentDictionary<long, DateTime> _pendingKilledMobs = new();
    private readonly ConcurrentDictionary<long, byte> _localPlayerDamagedMobs = new();

    public void TrackLocalPlayerMobDamage(long mobObjectId, long causerId, double healthChange)
    {
        if (!trackingController.IsTrackingAllowedByMainCharacter() || healthChange >= 0 || _localPlayerDamagedMobs.ContainsKey(mobObjectId))
        {
            return;
        }

        if (!IsLocalPlayer(causerId))
        {
            return;
        }

        _localPlayerDamagedMobs.TryAdd(mobObjectId, 0);
    }

    public void TryAddMobKill(long mobObjectId, CombatMobCacheEntry mob, double healthChange, bool hasNewHealthValue)
    {
        if (!trackingController.IsTrackingAllowedByMainCharacter())
        {
            return;
        }

        if (healthChange >= 0 || hasNewHealthValue)
        {
            return;
        }

        if (mob == null)
        {
            if (_localPlayerDamagedMobs.ContainsKey(mobObjectId))
            {
                AddPendingMobKill(mobObjectId);
            }

            return;
        }

        TryRecordMobKill(mobObjectId, mob);
    }

    public bool TryAddPendingMobKill(long mobObjectId, CombatMobCacheEntry mob)
    {
        if (!trackingController.IsTrackingAllowedByMainCharacter() || mob == null)
        {
            return false;
        }

        RemoveExpiredPendingMobKills();

        if (!_pendingKilledMobs.TryGetValue(mobObjectId, out var pendingKillTimestamp)
            || DateTime.UtcNow - pendingKillTimestamp > PendingMobKillRetention)
        {
            _pendingKilledMobs.TryRemove(mobObjectId, out _);
            return false;
        }

        if (!_pendingKilledMobs.TryRemove(mobObjectId, out _))
        {
            return false;
        }

        return TryRecordMobKill(mobObjectId, mob);
    }

    public void ResetRecordedMobKill(long mobObjectId)
    {
        _recordedKilledMobs.TryRemove(mobObjectId, out _);
        _localPlayerDamagedMobs.TryRemove(mobObjectId, out _);
    }

    private bool TryRecordMobKill(long mobObjectId, CombatMobCacheEntry mob)
    {
        if (mob.MobData == null || string.IsNullOrWhiteSpace(mob.MobData.UniqueName))
        {
            return false;
        }

        if (string.Equals(mob.MobData.UniqueName, UnknownMobUniqueName, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!_localPlayerDamagedMobs.ContainsKey(mobObjectId))
        {
            return false;
        }

        if (!_recordedKilledMobs.TryAdd(mobObjectId, 0))
        {
            return false;
        }

        _localPlayerDamagedMobs.TryRemove(mobObjectId, out _);

        var mobUniqueName = mob.MobData.UniqueName ?? mob.UniqueName ?? string.Empty;
        trackingController.StatisticController.AddMobKill(mobUniqueName);
        return true;
    }

    private void AddPendingMobKill(long mobObjectId)
    {
        RemoveExpiredPendingMobKills();
        _pendingKilledMobs[mobObjectId] = DateTime.UtcNow;
        Log.Debug("Mob kill pending until mob data is available | ObjectId={ObjectId}", mobObjectId);
    }

    private void RemoveExpiredPendingMobKills()
    {
        var currentUtc = DateTime.UtcNow;
        foreach (var pendingKilledMob in _pendingKilledMobs.ToArray())
        {
            if (currentUtc - pendingKilledMob.Value > PendingMobKillRetention)
            {
                _pendingKilledMobs.TryRemove(pendingKilledMob.Key, out _);
            }
        }
    }

    private bool IsLocalPlayer(long objectId)
    {
        if (trackingController.EntityController.LocalUserData.UserObjectId == objectId)
        {
            return true;
        }

        var localEntity = trackingController.EntityController.GetLocalEntity();
        return localEntity?.Value?.ObjectId == objectId;
    }
}