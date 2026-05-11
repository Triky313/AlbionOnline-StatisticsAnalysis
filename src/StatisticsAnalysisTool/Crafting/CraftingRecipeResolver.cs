using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Models.ItemsJsonModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CommonCraftingController = StatisticsAnalysisTool.Common.CraftingController;

namespace StatisticsAnalysisTool.Crafting;

public class CraftingRecipeResolver
{
    public bool IsCraftable(Item item)
    {
        return GetCraftingRequirements(item)?.CraftResource?.Count > 0;
    }

    public int GetAmountCrafted(Item item)
    {
        var craftingRequirements = GetCraftingRequirements(item);
        if (int.TryParse(craftingRequirements?.AmountCrafted, NumberStyles.Integer, CultureInfo.InvariantCulture, out var amountCrafted)
            && amountCrafted > 0)
        {
            return amountCrafted;
        }

        return 1;
    }

    public List<CraftingResourceEntry> GetResources(Item item)
    {
        var craftingRequirements = GetCraftingRequirements(item);
        if (craftingRequirements?.CraftResource == null)
        {
            return [];
        }

        return craftingRequirements.CraftResource
            .Where(x => !string.IsNullOrWhiteSpace(x.UniqueName))
            .GroupBy(x => x.UniqueName, StringComparer.Ordinal)
            .Select(x => CreateResourceEntry(item, x.Key, x.ToList()))
            .Where(x => x != null)
            .ToList();
    }

    public CraftingJournalEntry GetJournal(Item item)
    {
        if (item == null)
        {
            return null;
        }

        var journalItem = item.FullItemInformation switch
        {
            Weapon weapon => CommonCraftingController.GetCraftingJournalItem(item.Tier, weapon.CraftingJournalType),
            TransformationWeapon transformationWeapon => CommonCraftingController.GetCraftingJournalItem(item.Tier, transformationWeapon.CraftingJournalType),
            EquipmentItem equipmentItem => CommonCraftingController.GetCraftingJournalItem(item.Tier, equipmentItem.CraftingJournalType),
            TrackingItem trackingItem => CommonCraftingController.GetCraftingJournalItem(item.Tier, trackingItem.CraftingJournalType),
            _ => null
        }
        ;

        if (journalItem == null)
        {
            return null;
        }

        var generalJournalName = ItemController.GetGeneralJournalName(journalItem.UniqueName);
        var fullJournalUniqueName = journalItem.UniqueName.Replace("_EMPTY", "_FULL", StringComparison.Ordinal);
        var generalJournalItem = ItemController.GetItemByUniqueName(generalJournalName);
        var resources = CommonCraftingController.GetTotalAmountResources([GetCraftingRequirements(item)]);
        var famePerRun = CommonCraftingController.GetTotalBaseFame(resources, (ItemTier) item.Tier, (ItemLevel) item.Level);
        var maxFamePerJournal = GetMaxJournalFame(item.Tier);

        if (famePerRun <= 0d || maxFamePerJournal <= 0m)
        {
            return null;
        }

        return new CraftingJournalEntry
        {
            EmptyJournalUniqueName = journalItem.UniqueName,
            FullJournalUniqueName = fullJournalUniqueName,
            DisplayName = journalItem.LocalizedName,
            FamePerRun = (decimal) famePerRun,
            MaxFamePerJournal = maxFamePerJournal,
            UnitWeight = ItemController.GetWeight(generalJournalItem?.FullItemInformation),
            Icon = journalItem.Icon
        }
        ;
    }

    public CraftingRequirements GetCraftingRequirements(Item item)
    {
        if (item == null)
        {
            return null;
        }

        var enchantments = item.FullItemInformation switch
        {
            Weapon weapon => weapon.Enchantments,
            EquipmentItem equipmentItem => equipmentItem.Enchantments,
            ConsumableItem consumableItem => consumableItem.Enchantments,
            TransformationWeapon transformationWeapon => transformationWeapon.Enchantments,
            _ => null
        };

        var enchantment = enchantments?.Enchantment?.FirstOrDefault(x => x.EnchantmentLevelInteger == item.Level);
        if (enchantment?.CraftingRequirements?.FirstOrDefault()?.CraftResource.Count > 0)
        {
            return enchantment.CraftingRequirements.FirstOrDefault();
        }

        return item.FullItemInformation switch
        {
            Weapon weapon => weapon.CraftingRequirements?.FirstOrDefault(),
            TransformationWeapon transformationWeapon => transformationWeapon.CraftingRequirements?.FirstOrDefault(),
            EquipmentItem equipmentItem => equipmentItem.CraftingRequirements?.FirstOrDefault(),
            Mount mount => mount.CraftingRequirements?.FirstOrDefault(),
            ConsumableItem consumableItem => consumableItem.CraftingRequirements?.FirstOrDefault(),
            TrackingItem trackingItem => trackingItem.CraftingRequirements?.FirstOrDefault(),
            _ => null
        };
    }

    private static CraftingResourceEntry CreateResourceEntry(Item craftedItem, string uniqueName, List<CraftResource> resources)
    {
        var item = GetSuitableResourceItem(uniqueName, craftedItem.Level);
        if (item == null)
        {
            return null;
        }

        var quantityPerRun = resources.Sum(x => x.Count);
        var maxReturnQuantityPerRun = resources
            .Select(ParseNullableDecimal)
            .Where(x => x is > 0m)
            .Sum();
        var resourceKind = GetResourceKind(item);

        return new CraftingResourceEntry
        {
            UniqueName = item.UniqueName,
            DisplayName = item.LocalizedName,
            QuantityPerRun = quantityPerRun,
            UnitWeight = ItemController.GetWeight(item.FullItemInformation),
            ResourceKind = resourceKind,
            IsReturnable = IsReturnable(resourceKind),
            MaxReturnQuantityPerRun = maxReturnQuantityPerRun > 0m ? maxReturnQuantityPerRun : null,
            Icon = item.Icon
        }
        ;
    }

    private static decimal? ParseNullableDecimal(CraftResource craftResource)
    {
        if (decimal.TryParse(craftResource?.MaxReturnAmount, NumberStyles.Number, CultureInfo.InvariantCulture, out var value))
        {
            return value;
        }

        return null;
    }

    private static Item GetSuitableResourceItem(string uniqueName, int craftedItemLevel)
    {
        var item = ItemController.GetItemByUniqueName(uniqueName);
        if (item != null)
        {
            return item;
        }

        var suitableUniqueName = uniqueName + "_LEVEL" + craftedItemLevel + "@" + craftedItemLevel;
        return ItemController.GetItemByUniqueName(suitableUniqueName);
    }

    private static CraftingResourceKind GetResourceKind(Item item)
    {
        var uniqueName = item?.UniqueName?.ToUpperInvariant() ?? string.Empty;

        if (uniqueName.Contains("ARTEFACT"))
        {
            return CraftingResourceKind.Artefact;
        }

        if (uniqueName.Contains("_FAVOR_") || uniqueName.Contains("TOKEN_FAVOR"))
        {
            return CraftingResourceKind.Favor;
        }

        if (uniqueName.Contains("_ALCHEMY_"))
        {
            return CraftingResourceKind.Alchemy;
        }

        if (uniqueName.Contains("QUESTITEM_TOKEN_AVALON"))
        {
            return CraftingResourceKind.AvalonianEnergy;
        }

        if (uniqueName.Contains("SKILLBOOK_STANDARD"))
        {
            return CraftingResourceKind.TomeOfInsight;
        }

        if (item?.FullItemInformation is SimpleItem {ResourceType: "ESSENCE"})
        {
            return CraftingResourceKind.Essence;
        }

        return CraftingResourceKind.Standard;
    }

    private static bool IsReturnable(CraftingResourceKind resourceKind)
    {
        return resourceKind == CraftingResourceKind.Standard;
    }

    private static decimal GetMaxJournalFame(int tier)
    {
        return tier switch
        {
            2 => (decimal) CraftingJournalFame.T2,
            3 => (decimal) CraftingJournalFame.T3,
            4 => (decimal) CraftingJournalFame.T4,
            5 => (decimal) CraftingJournalFame.T5,
            6 => (decimal) CraftingJournalFame.T6,
            7 => (decimal) CraftingJournalFame.T7,
            8 => (decimal) CraftingJournalFame.T8,
            _ => 0m
        }
        ;
    }
}