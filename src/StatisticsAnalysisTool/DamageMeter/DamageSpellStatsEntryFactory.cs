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
            .Select(entry => new
            {
                Entry = entry,
                SpellIndex = SpellPresentationResolver.ResolveSpellIndex(entry.SpellIndex)
            })
            .Where(entry => entry.Entry.Value > 0)
            .GroupBy(entry => entry.SpellIndex)
            .Select(group => new DamageSpellStatsEntry
            {
                SpellIndex = group.Key,
                UniqueName = ResolveUniqueName(group.Key, group.Select(entry => entry.Entry)),
                Value = group.Sum(entry => entry.Entry.Value)
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
            .Take(10)
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
        var resolvedUniqueName = spellIndex <= 0 ? "AUTO_ATTACK" : SpellData.GetUniqueName(spellIndex);
        if (!string.IsNullOrWhiteSpace(resolvedUniqueName))
        {
            return resolvedUniqueName;
        }

        var uniqueName = entries
            .Select(entry => entry.UniqueName)
            .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
        return uniqueName ?? string.Empty;
    }
}