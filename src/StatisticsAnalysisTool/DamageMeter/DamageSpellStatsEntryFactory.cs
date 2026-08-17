using StatisticsAnalysisTool.GameFileData;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StatisticsAnalysisTool.DamageMeter;

internal static class DamageSpellStatsEntryFactory
{
    public static IReadOnlyList<DamageSpellStatsEntry> Rank(IEnumerable<DamageSpellStatsEntry> entries)
    {
        var orderedEntries = (entries ?? [])
            .Where(entry => entry.Value > 0)
            .GroupBy(entry => entry.SpellIndex)
            .Select(group => new DamageSpellStatsEntry
            {
                SpellIndex = group.Key,
                UniqueName = ResolveUniqueName(group.Key, group),
                Value = group.Sum(entry => entry.Value)
            })
            .OrderByDescending(entry => entry.Value)
            .ThenBy(entry => entry.SpellIndex)
            .ToList();
        if (orderedEntries.Count == 0)
        {
            return [];
        }

        var maximum = orderedEntries[0].Value;
        var total = orderedEntries.Sum(entry => entry.Value);
        return orderedEntries
            .Take(5)
            .Select((entry, index) => new DamageSpellStatsEntry
            {
                Rank = index + 1,
                SpellIndex = entry.SpellIndex,
                UniqueName = entry.UniqueName,
                Value = entry.Value,
                BarPercentage = Math.Min(100, (double) entry.Value / maximum * 100),
                SharePercentage = Math.Min(100, (double) entry.Value / total * 100)
            })
            .ToList();
    }

    private static string ResolveUniqueName(int spellIndex, IEnumerable<DamageSpellStatsEntry> entries)
    {
        var uniqueName = entries
            .Select(entry => entry.UniqueName)
            .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
        if (!string.IsNullOrWhiteSpace(uniqueName))
        {
            return uniqueName;
        }

        return spellIndex <= 0 ? "AUTO_ATTACK" : SpellData.GetUniqueName(spellIndex);
    }
}