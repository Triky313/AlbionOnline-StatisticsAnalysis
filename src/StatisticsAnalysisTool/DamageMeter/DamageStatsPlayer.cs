using System;
using System.Collections.Generic;
using StatisticsAnalysisTool.Enumerations;

namespace StatisticsAnalysisTool.DamageMeter;

internal sealed class DamageStatsPlayer
{
    public Guid PlayerGuid { get; init; }
    public string PlayerName { get; set; } = string.Empty;
    public long BiggestHit { get; set; }
    public long BiggestHeal { get; set; }
    public long TotalDamage { get; set; }
    public long EffectiveHealing { get; set; }
    public long Overheal { get; set; }
    public RollingDamageWindow BurstDamageFiveSeconds { get; } = new(TimeSpan.FromSeconds(5));
    public RollingDamageWindow BurstDamageTenSeconds { get; } = new(TimeSpan.FromSeconds(10));
    public Dictionary<DamageType, long> DamageByType { get; } = [];
    public Dictionary<int, long> DamageBySpellIndex { get; } = [];
    public HashSet<long> LastHitTargetObjectIds { get; } = [];
    public HashSet<long> MobLastHitTargetObjectIds { get; } = [];
    public HashSet<long> AttackedTargetObjectIds { get; } = [];
}