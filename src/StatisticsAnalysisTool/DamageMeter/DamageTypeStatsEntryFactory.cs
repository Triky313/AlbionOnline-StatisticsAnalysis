using StatisticsAnalysisTool.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StatisticsAnalysisTool.DamageMeter;

internal static class DamageTypeStatsEntryFactory
{
    private static readonly DamageType[] TrackedDamageTypes =
    [
        DamageType.Physical,
        DamageType.Magic,
        DamageType.True
    ];

    public static IReadOnlyList<DamageTypeStatsEntry> Rank(IEnumerable<DamageTypeStatsEntry> entries)
    {
        var valuesByType = (entries ?? [])
            .Where(entry => TrackedDamageTypes.Contains(entry.DamageType))
            .GroupBy(entry => entry.DamageType)
            .ToDictionary(group => group.Key, group => group.Sum(entry => entry.Value));
        var orderedEntries = TrackedDamageTypes
            .Select(damageType => new DamageTypeStatsEntry
            {
                DamageType = damageType,
                Value = valuesByType.GetValueOrDefault(damageType)
            })
            .OrderByDescending(entry => entry.Value)
            .ThenBy(entry => entry.DamageType)
            .ToList();

        var maximum = orderedEntries[0].Value;
        var total = orderedEntries.Sum(entry => entry.Value);
        return orderedEntries
            .Select((entry, index) => new DamageTypeStatsEntry
            {
                Rank = index + 1,
                DamageType = entry.DamageType,
                Value = entry.Value,
                BarPercentage = maximum > 0 ? Math.Min(100, (double) entry.Value / maximum * 100) : 0,
                SharePercentage = total > 0 ? Math.Min(100, (double) entry.Value / total * 100) : 0
            })
            .ToList();
    }
}