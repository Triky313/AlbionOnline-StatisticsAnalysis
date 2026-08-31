using StatisticsAnalysisTool.GameFileData.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace StatisticsAnalysisTool.GameFileData;

internal static class SpellPresentationResolver
{
    private static IReadOnlyDictionary<string, IReadOnlyCollection<string>> _parentSpellUniqueNames = new Dictionary<string, IReadOnlyCollection<string>>(StringComparer.Ordinal);
    private static ConcurrentDictionary<string, IReadOnlyDictionary<string, int>> _ancestorDistancesByUniqueName = new(StringComparer.Ordinal);

    internal static void Initialize(IReadOnlyCollection<XElement> elements, IReadOnlyCollection<GameFileDataSpell> spells)
    {
        var knownSpellUniqueNames = spells
            .Where(spell => !string.IsNullOrWhiteSpace(spell.UniqueName))
            .Select(spell => spell.UniqueName)
            .ToHashSet(StringComparer.Ordinal);
        var parentSpellUniqueNames = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);

        foreach (var element in elements)
        {
            var parentUniqueName = element.Attribute("uniquename")?.Value;
            if (string.IsNullOrWhiteSpace(parentUniqueName))
            {
                continue;
            }

            foreach (var referencedUniqueName in element
                         .DescendantsAndSelf()
                         .Attributes()
                         .Select(attribute => attribute.Value)
                         .Where(value => knownSpellUniqueNames.Contains(value)
                                         && !string.Equals(value, parentUniqueName, StringComparison.Ordinal)))
            {
                if (!parentSpellUniqueNames.TryGetValue(referencedUniqueName, out var parents))
                {
                    parents = new HashSet<string>(StringComparer.Ordinal);
                    parentSpellUniqueNames.Add(referencedUniqueName, parents);
                }

                parents.Add(parentUniqueName);
            }
        }

        _parentSpellUniqueNames = parentSpellUniqueNames.ToDictionary(entry => entry.Key, 
            entry => (IReadOnlyCollection<string>) entry.Value.ToArray(),
            StringComparer.Ordinal);
        _ancestorDistancesByUniqueName = new ConcurrentDictionary<string, IReadOnlyDictionary<string, int>>(StringComparer.Ordinal);
    }

    public static int ResolveSpellIndex(int sourceSpellIndex, IEnumerable<int> preferredSpellIndexes = null)
    {
        var sourceSpell = SpellData.GetSpellByIndex(sourceSpellIndex);
        if (string.IsNullOrWhiteSpace(sourceSpell.UniqueName))
        {
            return sourceSpellIndex;
        }

        var ancestorDistances = GetAncestorDistances(sourceSpell.UniqueName);
        var preferredSpell = ResolvePreferredSpell(sourceSpell, ancestorDistances, preferredSpellIndexes);
        if (preferredSpell != null)
        {
            return preferredSpell.Index;
        }

        var relatedPresentationSpell = ResolveRelatedPresentationSpell(sourceSpell, ancestorDistances);
        return relatedPresentationSpell?.Index ?? sourceSpellIndex;
    }

    public static string ResolveUniqueName(int sourceSpellIndex, string fallbackUniqueName = null)
    {
        var resolvedUniqueName = SpellData.GetUniqueName(ResolveSpellIndex(sourceSpellIndex));
        return !string.IsNullOrWhiteSpace(resolvedUniqueName) ? resolvedUniqueName : fallbackUniqueName ?? string.Empty;
    }

    public static bool IsPresentationSpellFor(int presentationSpellIndex, int sourceSpellIndex)
    {
        if (presentationSpellIndex == sourceSpellIndex)
        {
            return true;
        }

        var presentationSpellUniqueName = SpellData.GetUniqueName(presentationSpellIndex);
        var sourceSpellUniqueName = SpellData.GetUniqueName(sourceSpellIndex);
        if (string.IsNullOrWhiteSpace(presentationSpellUniqueName) || string.IsNullOrWhiteSpace(sourceSpellUniqueName))
        {
            return false;
        }

        return GetAncestorDistances(sourceSpellUniqueName).ContainsKey(presentationSpellUniqueName);
    }

    private static GameFileDataSpell ResolvePreferredSpell(GameFileDataSpell sourceSpell, IReadOnlyDictionary<string, int> ancestorDistances, IEnumerable<int> preferredSpellIndexes)
    {
        var preferredSpells = (preferredSpellIndexes ?? [])
            .Distinct()
            .Select((spellIndex, order) => new
            {
                Spell = SpellData.GetSpellByIndex(spellIndex),
                Order = order
            })
            .Where(candidate => !string.IsNullOrWhiteSpace(candidate.Spell.UniqueName))
            .ToList();
        var relatedSpell = preferredSpells
            .Where(candidate => ancestorDistances.ContainsKey(candidate.Spell.UniqueName))
            .Select(candidate => new
            {
                candidate.Spell,
                candidate.Order,
                Distance = ancestorDistances[candidate.Spell.UniqueName],
                HasMatchingLocalization = HasMatchingLocalization(sourceSpell, candidate.Spell)
            })
            .OrderBy(candidate => candidate.Distance)
            .ThenByDescending(candidate => candidate.HasMatchingLocalization)
            .ThenBy(candidate => candidate.Order)
            .Select(candidate => candidate.Spell)
            .FirstOrDefault();
        if (relatedSpell != null)
        {
            return relatedSpell;
        }

        return preferredSpells
            .Where(candidate => sourceSpell.UniqueName.StartsWith(candidate.Spell.UniqueName, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(candidate => candidate.Spell.UniqueName.Length)
            .ThenByDescending(candidate => HasMatchingLocalization(sourceSpell, candidate.Spell))
            .ThenBy(candidate => candidate.Order)
            .Select(candidate => candidate.Spell)
            .FirstOrDefault();
    }

    private static GameFileDataSpell ResolveRelatedPresentationSpell(GameFileDataSpell sourceSpell, IReadOnlyDictionary<string, int> ancestorDistances)
    {
        var relatedSpells = ancestorDistances
            .Where(entry => entry.Value > 0)
            .Select(entry => (Spell: SpellData.GetSpellByUniqueName(entry.Key), Distance: entry.Value))
            .Where(candidate => !string.IsNullOrWhiteSpace(candidate.Spell.UniqueName))
            .ToList();

        var matchingLocalizationSpell = SelectUniqueNearestSpell(relatedSpells.Where(candidate => HasMatchingLocalization(sourceSpell, candidate.Spell)));
        if (matchingLocalizationSpell != null)
        {
            return matchingLocalizationSpell;
        }

        if (sourceSpell.HasIcon && HasLocalizedName(sourceSpell))
        {
            return null;
        }

        var localizedPresentationSpell = SelectUniqueNearestSpell(relatedSpells.Where(candidate => candidate.Spell.HasIcon && HasLocalizedName(candidate.Spell)));
        if (localizedPresentationSpell != null)
        {
            return localizedPresentationSpell;
        }

        if (sourceSpell.HasIcon)
        {
            return null;
        }

        return SelectUniqueNearestSpell(relatedSpells.Where(candidate => candidate.Spell.HasIcon));
    }

    private static GameFileDataSpell SelectUniqueNearestSpell(IEnumerable<(GameFileDataSpell Spell, int Distance)> candidates)
    {
        var candidateList = candidates.ToList();
        if (candidateList.Count == 0)
        {
            return null;
        }

        var minimumDistance = candidateList.Min(candidate => candidate.Distance);
        var nearestSpells = candidateList
            .Where(candidate => candidate.Distance == minimumDistance)
            .Select(candidate => candidate.Spell)
            .GroupBy(spell => spell.UniqueName, StringComparer.Ordinal)
            .Select(group => group.First())
            .ToList();

        return nearestSpells.Count == 1 ? nearestSpells[0] : null;
    }

    private static IReadOnlyDictionary<string, int> GetAncestorDistances(string sourceSpellUniqueName)
    {
        return _ancestorDistancesByUniqueName.GetOrAdd(sourceSpellUniqueName, BuildAncestorDistances);
    }

    private static IReadOnlyDictionary<string, int> BuildAncestorDistances(string sourceSpellUniqueName)
    {
        var distances = new Dictionary<string, int>(StringComparer.Ordinal)
        {
            [sourceSpellUniqueName] = 0
        };

        var pendingSpellUniqueNames = new Queue<string>();
        pendingSpellUniqueNames.Enqueue(sourceSpellUniqueName);

        while (pendingSpellUniqueNames.Count > 0)
        {
            var childUniqueName = pendingSpellUniqueNames.Dequeue();
            if (!_parentSpellUniqueNames.TryGetValue(childUniqueName, out var parentUniqueNames))
            {
                continue;
            }

            var parentDistance = distances[childUniqueName] + 1;
            foreach (var parentUniqueName in parentUniqueNames)
            {
                if (distances.TryGetValue(parentUniqueName, out var existingDistance)
                    && existingDistance <= parentDistance)
                {
                    continue;
                }

                distances[parentUniqueName] = parentDistance;
                pendingSpellUniqueNames.Enqueue(parentUniqueName);
            }
        }

        return distances;
    }

    private static bool HasMatchingLocalization(GameFileDataSpell sourceSpell, GameFileDataSpell candidateSpell)
    {
        return string.Equals(GetNameLocalizationKey(sourceSpell), GetNameLocalizationKey(candidateSpell), StringComparison.Ordinal);
    }
    private static bool HasLocalizedName(GameFileDataSpell spell)
    {
        var localizedName = SpellData.GetLocalizationName(spell.UniqueName);
        return !string.IsNullOrWhiteSpace(localizedName) && !string.Equals(localizedName, spell.UniqueName, StringComparison.Ordinal);
    }


    private static string GetNameLocalizationKey(GameFileDataSpell spell)
    {
        return string.IsNullOrWhiteSpace(spell.NameLocatag) ? $"@SPELLS_{spell.UniqueName}" : spell.NameLocatag;
    }
}