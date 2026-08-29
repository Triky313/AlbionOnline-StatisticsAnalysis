using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Models.ItemsJsonModel;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace StatisticsAnalysisTool.Trade;

internal static class BuildingTradeClassifier
{
    public static bool IsMerchantPurchase(int itemIndex)
    {
        var itemInformation = ItemController.GetItemByIndex(itemIndex)?.FullItemInformation;
        return itemInformation switch
        {
            HideoutItem hideoutItem => HasSilverOnlyPurchaseRequirement(hideoutItem.CraftingRequirements),
            FarmableItem farmableItem => HasSilverOnlyPurchaseRequirement(farmableItem.CraftingRequirements),
            SimpleItem simpleItem => HasSilverOnlyPurchaseRequirement(simpleItem.CraftingRequirements),
            ConsumableItem consumableItem => HasSilverOnlyPurchaseRequirement(consumableItem.CraftingRequirements),
            ConsumableFromInventoryItem consumableFromInventoryItem => HasSilverOnlyPurchaseRequirement(consumableFromInventoryItem.CraftingRequirements),
            EquipmentItem equipmentItem => HasSilverOnlyPurchaseRequirement(equipmentItem.CraftingRequirements),
            Weapon weapon => HasSilverOnlyPurchaseRequirement(weapon.CraftingRequirements),
            Mount mount => HasSilverOnlyPurchaseRequirement(mount.CraftingRequirements),
            FurnitureItem furnitureItem => HasSilverOnlyPurchaseRequirement(furnitureItem.CraftingRequirements),
            JournalItem journalItem => HasSilverOnlyPurchaseRequirement(journalItem.CraftingRequirements),
            TransformationWeapon transformationWeapon => HasSilverOnlyPurchaseRequirement(transformationWeapon.CraftingRequirements),
            _ => false
        };
    }

    private static bool HasSilverOnlyPurchaseRequirement(IEnumerable<CraftingRequirements> craftingRequirements)
    {
        if (craftingRequirements is null)
        {
            return false;
        }

        return craftingRequirements.Any(IsSilverOnlyPurchaseRequirement);
    }

    private static bool IsSilverOnlyPurchaseRequirement(CraftingRequirements craftingRequirement)
    {
        return long.TryParse(
                   craftingRequirement.Silver,
                   NumberStyles.Integer,
                   CultureInfo.InvariantCulture,
                   out var silver)
               && silver > 0
               && double.TryParse(
                   craftingRequirement.Time,
                   NumberStyles.Float,
                   CultureInfo.InvariantCulture,
                   out var duration)
               && duration == 0
               && (craftingRequirement.CraftResource is null || craftingRequirement.CraftResource.Count == 0);
    }
}