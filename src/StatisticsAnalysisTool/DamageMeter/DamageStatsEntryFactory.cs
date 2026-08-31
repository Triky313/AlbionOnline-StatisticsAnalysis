using System;
using System.Collections.Generic;
using System.Linq;

namespace StatisticsAnalysisTool.DamageMeter;

internal static class DamageStatsEntryFactory
{
    private const int TopCount = 5;

    public static IReadOnlyList<DamageStatsEntry> Rank(
        IEnumerable<DamageStatsEntry> entries,
        bool calculateSharePercentage = false)
    {
        var orderedEntries = (entries ?? [])
            .Where(x => x.Value > 0)
            .OrderByDescending(x => x.Value)
            .ThenBy(x => x.PlayerName)
            .ToList();

        if (orderedEntries.Count == 0)
        {
            return [];
        }

        var maximum = orderedEntries[0].Value;
        var total = orderedEntries.Sum(x => (double) x.Value);

        return orderedEntries
            .Take(TopCount)
            .Select((entry, index) => new DamageStatsEntry
            {
                Rank = index + 1,
                PlayerName = entry.PlayerName,
                Value = entry.Value,
                BarPercentage = maximum > 0 ? Math.Min(100, (double) entry.Value / maximum * 100) : 0,
                SharePercentage = calculateSharePercentage && total > 0
                    ? Math.Min(100, entry.Value / total * 100)
                    : 0,
                Detail = entry.Detail
            })
            .ToList();
    }
}