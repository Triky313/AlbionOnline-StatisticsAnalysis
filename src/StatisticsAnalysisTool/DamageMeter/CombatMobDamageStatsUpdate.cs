using System.Collections.Generic;

namespace StatisticsAnalysisTool.DamageMeter;

public sealed class CombatMobDamageStatsUpdate
{
    public long Version { get; init; }
    public long TotalDamage { get; init; }
    public IReadOnlyCollection<CombatMobDamageStats> ChangedMobs { get; init; } = [];
}