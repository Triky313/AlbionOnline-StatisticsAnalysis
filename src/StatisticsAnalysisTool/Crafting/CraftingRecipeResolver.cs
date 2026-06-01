using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Models.ItemsJsonModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

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
            .GroupBy(x => (x.UniqueName, EnchantmentLevel: GetResourceEnchantmentLevel(x)))
            .Select(x => CreateResourceEntry(item, x.Key.UniqueName, x.Key.EnchantmentLevel, x.ToList()))
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
            Weapon weapon => CraftingJournalService.GetCraftingJournalItem(item.Tier, weapon.CraftingJournalType),
            TransformationWeapon transformationWeapon => CraftingJournalService.GetCraftingJournalItem(item.Tier, transformationWeapon.CraftingJournalType),
            EquipmentItem equipmentItem => CraftingJournalService.GetCraftingJournalItem(item.Tier, equipmentItem.CraftingJournalType),
            TrackingItem trackingItem => CraftingJournalService.GetCraftingJournalItem(item.Tier, trackingItem.CraftingJournalType),
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
        var resources = CraftingJournalService.GetTotalAmountResources([GetCraftingRequirements(item)]);
        var famePerRun = CraftingJournalService.GetTotalBaseFame(resources, (ItemTier) item.Tier, (ItemLevel) item.Level);
        var maxFamePerJournal = GetMaxJournalFame(item.Tier);

        if (famePerRun <= 0d || maxFamePerJournal <= 0m)
        {
            return null;
        }

        return new CraftingJournalEntry
        {
            EmptyJournalUniqueName = journalItem.UniqueName,
            FullJournalUniqueName = fullJournalUniqueName,
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

    private static CraftingResourceEntry CreateResourceEntry(Item craftedItem, string uniqueName, int? resourceEnchantmentLevel, List<CraftResource> resources)
    {
        var item = GetSuitableResourceItem(uniqueName, resourceEnchantmentLevel, craftedItem.Level);
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

    private static int? GetResourceEnchantmentLevel(CraftResource craftResource)
    {
        if (int.TryParse(craftResource?.EnchantmentLevel, NumberStyles.Integer, CultureInfo.InvariantCulture, out var enchantmentLevel))
        {
            return enchantmentLevel > 0 ? enchantmentLevel : null;
        }

        return null;
    }

    internal static Item GetSuitableResourceItem(string uniqueName, int? resourceEnchantmentLevel, int craftedItemLevel)
    {
        if (resourceEnchantmentLevel is > 0 && !HasEnchantmentSuffix(uniqueName))
        {
            var enchantedResourceItem = GetEnchantedResourceItem(uniqueName, resourceEnchantmentLevel.Value);
            if (enchantedResourceItem != null)
            {
                return enchantedResourceItem;
            }
        }

        var item = ItemController.GetItemByUniqueName(uniqueName);
        if (item != null)
        {
            return item;
        }

        return GetEnchantedResourceItem(uniqueName, craftedItemLevel);
    }

    private static bool HasEnchantmentSuffix(string uniqueName)
    {
        return uniqueName.Contains('@', StringComparison.Ordinal) || uniqueName.Contains("_LEVEL", StringComparison.Ordinal);
    }

    private static Item GetEnchantedResourceItem(string uniqueName, int enchantmentLevel)
    {
        return ItemController.GetItemByUniqueName(GetEnchantedEquipmentUniqueName(uniqueName, enchantmentLevel))
               ?? ItemController.GetItemByUniqueName(GetEnchantedMaterialUniqueName(uniqueName, enchantmentLevel));
    }

    private static string GetEnchantedEquipmentUniqueName(string uniqueName, int enchantmentLevel)
    {
        return uniqueName + "@" + enchantmentLevel;
    }

    private static string GetEnchantedMaterialUniqueName(string uniqueName, int enchantmentLevel)
    {
        return uniqueName + "_LEVEL" + enchantmentLevel + "@" + enchantmentLevel;
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

        if (item?.FullItemInformation is SimpleItem { ResourceType: "ESSENCE" })
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
