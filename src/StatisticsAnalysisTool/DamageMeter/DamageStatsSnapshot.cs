using System;
using System.Collections.Generic;
using System.Linq;

namespace StatisticsAnalysisTool.DamageMeter;

public sealed class DamageStatsSnapshot
{
    public static DamageStatsSnapshot Empty { get; } = new();

    public IReadOnlyList<DamageStatsEntry> TopSingleHits { get; init; } = [];
    public IReadOnlyList<DamageStatsEntry> TopSingleHeals { get; init; } = [];
    public IReadOnlyList<DamageStatsEntry> TopTotalDamage { get; init; } = [];
    public IReadOnlyList<DamageStatsEntry> TopEffectiveHealing { get; init; } = [];
    public IReadOnlyList<DamageStatsEntry> TopLastHits { get; init; } = [];
    public IReadOnlyList<DamageStatsEntry> TopMobKillContribution { get; init; } = [];
    public IReadOnlyList<DamageStatsEntry> TopOverheals { get; init; } = [];
    public IReadOnlyList<DamageStatsEntry> TopTakenDamage { get; init; } = [];
    public IReadOnlyList<DamageStatsEntry> TopBurstDamageFiveSeconds { get; init; } = [];
    public IReadOnlyList<DamageStatsEntry> TopBurstDamageTenSeconds { get; init; } = [];
    public IReadOnlyList<DamageStatsEntry> TopAttackedTargets { get; init; } = [];
    public int TrackedFightCount { get; init; }
    public TimeSpan TrackedFightDuration { get; init; }

    public DamageStatsEntry BiggestHit => TopSingleHits.FirstOrDefault() ?? DamageStatsEntry.Empty;
    public DamageStatsEntry PeakDpsFiveSeconds => CreatePeakDpsEntry(TopBurstDamageFiveSeconds, 5);
    public DamageStatsEntry PeakDpsTenSeconds => CreatePeakDpsEntry(TopBurstDamageTenSeconds, 10);
    public DamageStatsEntry TopHealer => TopEffectiveHealing.FirstOrDefault() ?? DamageStatsEntry.Empty;
    public DamageStatsEntry MostDamageTaken => TopTakenDamage.FirstOrDefault() ?? DamageStatsEntry.Empty;

    public string TrackedFightDurationString
    {
        get
        {
            var totalHours = (int) TrackedFightDuration.TotalHours;
            if (totalHours > 0)
            {
                return $"{totalHours}h {TrackedFightDuration.Minutes}m";
            }

            return TrackedFightDuration.Minutes > 0
                ? $"{TrackedFightDuration.Minutes}m {TrackedFightDuration.Seconds}s"
                : $"{TrackedFightDuration.Seconds}s";
        }
    }

    private static DamageStatsEntry CreatePeakDpsEntry(
        IReadOnlyList<DamageStatsEntry> entries,
        int windowSeconds)
    {
        var entry = entries.FirstOrDefault();
        if (entry == null)
        {
            return DamageStatsEntry.Empty;
        }

        return new DamageStatsEntry
        {
            PlayerName = entry.PlayerName,
            Value = (long) Math.Round((double) entry.Value / windowSeconds, MidpointRounding.AwayFromZero)
        };
    }
}
