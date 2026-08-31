using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Models.ItemsJsonModel;

namespace StatisticsAnalysisTool.Common;

public static class BlackMarketItemEligibility
{
    public static bool IsEligible(Item item)
    {
        return item?.FullItemInformation switch
        {
            Weapon weapon => weapon.SlotTypeEnum == SlotType.MainHand && weapon.CanHarvest == null,
            TransformationWeapon transformationWeapon => transformationWeapon.SlotTypeEnum == SlotType.MainHand,
            EquipmentItem equipmentItem => equipmentItem.SlotTypeEnum is SlotType.OffHand
                or SlotType.Armor
                or SlotType.Head
                or SlotType.Shoes
                or SlotType.Cape
                or SlotType.Bag,
            _ => false
        };
    }
}
