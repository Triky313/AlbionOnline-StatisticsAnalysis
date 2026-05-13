using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Models;

namespace StatisticsAnalysisTool.Crafting;

public class CraftingStationFeeService
{
    private const decimal NutritionPerItemValue = 0.1125m;

    public decimal GetNutritionConsumedPerRun(Item item)
    {
        if (item?.FullItemInformation == null)
        {
            return 0m;
        }

        var itemValue = ItemController.GetItemValue(item.FullItemInformation, item.Level);
        if (itemValue <= 0d)
        {
            return 0m;
        }

        return (decimal) itemValue * NutritionPerItemValue;
    }
}