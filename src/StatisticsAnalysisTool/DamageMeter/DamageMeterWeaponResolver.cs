using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Models.NetworkModel;

namespace StatisticsAnalysisTool.DamageMeter;

internal static class DamageMeterWeaponResolver
{
    public static int GetEquippedWeaponItemIndex(PlayerGameObject player)
    {
        var itemIndex = player.CharacterEquipment?.MainHand ?? 0;
        return GetWeaponByIndex(itemIndex) != null ? itemIndex : 0;
    }

    public static Item GetWeaponByIndex(int itemIndex)
    {
        var item = ItemController.GetItemByIndex(itemIndex);
        return item?.FullItemInformation?.ItemType is ItemType.TransformationWeapon or ItemType.Weapon ? item : null;
    }
}