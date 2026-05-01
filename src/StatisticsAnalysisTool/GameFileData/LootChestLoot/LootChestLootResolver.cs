using StatisticsAnalysisTool.GameFileData.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StatisticsAnalysisTool.GameFileData.LootChestLoot;

public class LootChestLootResolver
{
    private const int MaximumDepth = 32;
    private const string DirectItemsPoolName = "Direct Items";
    private readonly IReadOnlyDictionary<string, LootListJsonObject> _lootListsByName;

    public LootChestLootResolver()
        : this(LootData.LootLists)
    {
    }

    public LootChestLootResolver(IEnumerable<LootListJsonObject> lootLists)
    {
        ArgumentNullException.ThrowIfNull(lootLists);

        _lootListsByName = lootLists
            .Where(x => !string.IsNullOrWhiteSpace(x.Name))
            .GroupBy(x => x.Name)
            .ToDictionary(x => x.Key, x => x.First());
    }

    public IReadOnlyList<LootChestDisplayEntry> ResolveLootChests(IEnumerable<LootChestJsonObject> lootChests)
    {
        ArgumentNullException.ThrowIfNull(lootChests);

        return lootChests
            .Where(x => !string.IsNullOrWhiteSpace(x.UniqueName))
            .OrderBy(x => x.UniqueName)
            .Select(x => new LootChestDisplayEntry(x, this))
            .ToList();
    }

    public IReadOnlyList<LootChestRareStateDisplayEntry> ResolveRareStates(LootChestJsonObject lootChest)
    {
        ArgumentNullException.ThrowIfNull(lootChest);

        var rareStates = lootChest.RareStates?.RareState ?? [];
        var totalWeight = rareStates.Sum(x => x.Weight);

        return rareStates
            .Select(x => new LootChestRareStateDisplayEntry
            {
                State = x.State,
                Weight = x.Weight,
                RelativeWeight = CalculateRelativeWeight(x.Weight, totalWeight),
                Tiers = ResolveTiers(x)
            })
            .ToList();
    }

    public static string BuildUniqueName(string type, int enchantmentLevel)
    {
        if (string.IsNullOrWhiteSpace(type))
        {
            return string.Empty;
        }

        return enchantmentLevel > 0
            ? $"{type}@{enchantmentLevel}"
            : type;
    }

    private IReadOnlyList<LootChestTierDisplayEntry> ResolveTiers(LootChestRareState rareState)
    {
        var tiers = new List<LootChestTierDisplayEntry>();

        if (rareState.Loot.Item.Count > 0 || rareState.Loot.LootListReference.Count > 0)
        {
            tiers.Add(new LootChestTierDisplayEntry
            {
                Tier = 0,
                Pools = ResolvePools(rareState.Loot.Item, rareState.Loot.LootListReference)
            });
        }

        tiers.AddRange(rareState.Loot.LootByTier.Select(x => new LootChestTierDisplayEntry
        {
            Tier = x.Tier,
            Pools = ResolvePools(x.Item, x.LootListReference)
        }));

        return tiers;
    }

    private IReadOnlyList<LootPoolDisplayEntry> ResolvePools(
        IReadOnlyList<LootItemJsonObject> directItems,
        IReadOnlyList<LootListReferenceJsonObject> references)
    {
        var pools = new List<LootPoolDisplayEntry>();

        if (directItems.Count > 0)
        {
            pools.Add(new LootPoolDisplayEntry
            {
                Name = DirectItemsPoolName,
                Items = directItems.Select(x => CreateItem(x, DirectItemsPoolName, 0)).ToList()
            });
        }

        foreach (var reference in references.Where(x => !string.IsNullOrWhiteSpace(x.Name)))
        {
            var items = ResolveLootListItems(reference.Name, reference.Name, 1, new HashSet<string>(), 0, reference.Chance, reference.Weight);

            pools.Add(new LootPoolDisplayEntry
            {
                Name = reference.Name,
                Chance = reference.Chance,
                Weight = reference.Weight,
                Items = items
            });
        }

        return pools;
    }

    private IReadOnlyList<LootItemDisplayEntry> ResolveLootListItems(
        string listName,
        string path,
        double relativeWeightFactor,
        IReadOnlySet<string> visitedLootLists,
        int depth,
        double inheritedChance,
        double inheritedWeight)
    {
        if (depth >= MaximumDepth || string.IsNullOrWhiteSpace(listName) || visitedLootLists.Contains(listName))
        {
            return [];
        }

        if (!_lootListsByName.TryGetValue(listName, out var lootList))
        {
            return [];
        }

        var nextVisitedLootLists = visitedLootLists.Append(listName).ToHashSet();
        var items = new List<LootItemDisplayEntry>();

        items.AddRange(lootList.Item.Select(x => CreateItem(x, path, relativeWeightFactor, inheritedChance, inheritedWeight)));

        foreach (var reference in lootList.LootListReference.Where(x => !string.IsNullOrWhiteSpace(x.Name)))
        {
            var nextRelativeWeightFactor = relativeWeightFactor * GetRelativeWeightFactor(reference.Weight, 0);
            var nextInheritedChance = reference.Chance > 0 ? reference.Chance : inheritedChance;
            var nextInheritedWeight = reference.Weight > 0 ? reference.Weight : inheritedWeight;
            var nextPath = AppendPath(path, reference.Name);
            items.AddRange(ResolveLootListItems(reference.Name, nextPath, nextRelativeWeightFactor, nextVisitedLootLists, depth + 1, nextInheritedChance, nextInheritedWeight));
        }

        foreach (var or in lootList.Or)
        {
            items.AddRange(ResolveOrItems(or, path, relativeWeightFactor, nextVisitedLootLists, depth + 1, inheritedChance, inheritedWeight));
        }

        return items;
    }

    private IReadOnlyList<LootItemDisplayEntry> ResolveOrItems(
        LootOrJsonObject lootOr,
        string path,
        double relativeWeightFactor,
        IReadOnlySet<string> visitedLootLists,
        int depth,
        double inheritedChance,
        double inheritedWeight)
    {
        if (depth >= MaximumDepth)
        {
            return [];
        }

        var items = new List<LootItemDisplayEntry>();

        if (lootOr.Or.Count > 0)
        {
            var totalWeight = lootOr.Or.Sum(x => x.Weight);

            foreach (var child in lootOr.Or)
            {
                var childRelativeWeightFactor = relativeWeightFactor * GetRelativeWeightFactor(child.Weight, totalWeight);
                var childInheritedChance = child.Chance > 0 ? child.Chance : inheritedChance;
                var childInheritedWeight = child.Weight > 0 ? child.Weight : inheritedWeight;
                items.AddRange(ResolveOrItems(child, path, childRelativeWeightFactor, visitedLootLists, depth + 1, childInheritedChance, childInheritedWeight));
            }

            return items;
        }

        var nextInheritedChance = lootOr.Chance > 0 ? lootOr.Chance : inheritedChance;
        var nextInheritedWeight = lootOr.Weight > 0 ? lootOr.Weight : inheritedWeight;

        items.AddRange(lootOr.Item.Select(x => CreateItem(x, path, relativeWeightFactor, nextInheritedChance, nextInheritedWeight)));

        foreach (var reference in lootOr.LootListReference.Where(x => !string.IsNullOrWhiteSpace(x.Name)))
        {
            var nextRelativeWeightFactor = relativeWeightFactor * GetRelativeWeightFactor(reference.Weight, 0);
            var referenceInheritedChance = reference.Chance > 0 ? reference.Chance : nextInheritedChance;
            var referenceInheritedWeight = reference.Weight > 0 ? reference.Weight : nextInheritedWeight;
            var nextPath = AppendPath(path, reference.Name);
            items.AddRange(ResolveLootListItems(reference.Name, nextPath, nextRelativeWeightFactor, visitedLootLists, depth + 1, referenceInheritedChance, referenceInheritedWeight));
        }

        return items;
    }

    private static LootItemDisplayEntry CreateItem(
        LootItemJsonObject item,
        string sourcePath,
        double relativeWeightFactor,
        double inheritedChance = 0,
        double inheritedWeight = 0)
    {
        var relativeWeight = relativeWeightFactor > 0 && relativeWeightFactor < 1
            ? relativeWeightFactor * 100
            : 0;

        return new LootItemDisplayEntry
        {
            UniqueName = BuildUniqueName(item.Type, item.EnchantmentLevel),
            Amount = item.Amount,
            Chance = item.Chance > 0 ? item.Chance : inheritedChance,
            Weight = item.Weight > 0 ? item.Weight : inheritedWeight,
            RelativeWeight = relativeWeight,
            SourcePath = sourcePath
        };
    }

    private static double CalculateRelativeWeight(double weight, double totalWeight)
    {
        return totalWeight > 0 && weight > 0
            ? weight / totalWeight * 100
            : 0;
    }

    private static double GetRelativeWeightFactor(double weight, double totalWeight)
    {
        if (totalWeight <= 0 || weight <= 0)
        {
            return 1;
        }

        return weight / totalWeight;
    }

    private static string AppendPath(string path, string name)
    {
        return string.IsNullOrWhiteSpace(path)
            ? name
            : $"{path} > {name}";
    }
}
