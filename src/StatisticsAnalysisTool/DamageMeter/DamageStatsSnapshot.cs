using System.Collections.Generic;

namespace StatisticsAnalysisTool.DamageMeter;

public sealed class DamageStatsSnapshot
{
    public static DamageStatsSnapshot Empty { get; } = new();

    public IReadOnlyList<DamageStatsEntry> TopSingleHits { get; init; } = [];
    public IReadOnlyList<DamageStatsEntry> TopSingleHeals { get; init; } = [];
    public IReadOnlyList<DamageStatsEntry> TopLastHits { get; init; } = [];
    public IReadOnlyList<DamageStatsEntry> TopOverheals { get; init; } = [];
    public IReadOnlyList<DamageStatsEntry> TopBurstDamageFiveSeconds { get; init; } = [];
    public IReadOnlyList<DamageStatsEntry> TopBurstDamageTenSeconds { get; init; } = [];
    public IReadOnlyList<DamageStatsEntry> TopAttackedTargets { get; init; } = [];
}