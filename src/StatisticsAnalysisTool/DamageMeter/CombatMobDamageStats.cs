using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Models;
using System;
using System.Collections.Generic;

namespace StatisticsAnalysisTool.DamageMeter;

public sealed class CombatMobDamageStats
{
    private readonly Dictionary<Guid, CombatMobPlayerDamageStats> _players = [];

    public Guid MobInstanceId { get; init; }
    public long MobObjectId { get; init; }
    public int MobIndex { get; private set; }
    public string UniqueName { get; private set; } = string.Empty;
    public string TypeId { get; private set; } = string.Empty;
    public string ClusterKey { get; init; } = string.Empty;
    public string ClusterName { get; init; } = string.Empty;
    public DashboardContentType ContentType { get; init; }
    public Tier MapTier { get; private set; } = Tier.Unknown;
    public DateTime FirstSeen { get; init; }
    public DateTime FirstDamageTime { get; private set; }
    public DateTime LastDamageTime { get; private set; }
    public double MaxHealth { get; private set; }
    public long Damage { get; private set; }
    public IReadOnlyCollection<CombatMobPlayerDamageStats> Players => _players.Values;

    internal void UpdateMob(CombatMobCacheEntry mob)
    {
        MobIndex = mob.MobIndex;
        UniqueName = mob.UniqueName;
        TypeId = mob.TypeId;
        MaxHealth = mob.MaxHealth;
        MapTier = mob.MapTier;
    }

    internal void RecordDamage(Guid? playerGuid, string playerName, int causingSpellIndex, long value, DateTime timestamp)
    {
        if (value <= 0)
        {
            return;
        }

        Damage += value;
        FirstDamageTime = FirstDamageTime == default ? timestamp : FirstDamageTime;
        LastDamageTime = timestamp;

        if (!playerGuid.HasValue)
        {
            return;
        }

        if (!_players.TryGetValue(playerGuid.Value, out var playerStats))
        {
            playerStats = new CombatMobPlayerDamageStats
            {
                PlayerGuid = playerGuid.Value
            };
            _players.Add(playerGuid.Value, playerStats);
        }

        playerStats.RecordDamage(playerName, causingSpellIndex, value, timestamp);
    }

    internal CombatMobDamageStats Clone()
    {
        var clone = new CombatMobDamageStats
        {
            MobInstanceId = MobInstanceId,
            MobObjectId = MobObjectId,
            MobIndex = MobIndex,
            UniqueName = UniqueName,
            TypeId = TypeId,
            ClusterKey = ClusterKey,
            ClusterName = ClusterName,
            ContentType = ContentType,
            MapTier = MapTier,
            FirstSeen = FirstSeen,
            FirstDamageTime = FirstDamageTime,
            LastDamageTime = LastDamageTime,
            MaxHealth = MaxHealth,
            Damage = Damage
        };

        foreach (var player in _players)
        {
            clone._players[player.Key] = player.Value.Clone();
        }

        return clone;
    }
}
