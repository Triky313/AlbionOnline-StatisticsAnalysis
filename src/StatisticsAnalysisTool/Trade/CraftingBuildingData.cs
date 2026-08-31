using System.Collections.Generic;

namespace StatisticsAnalysisTool.Trade;

public class CraftingBuildingData
{

    private static readonly Dictionary<CraftingBuildingName, string> CraftingBuildingNames = new()
    {
        { CraftingBuildingName.Forge, "FORGE" },
        { CraftingBuildingName.HuntersLodge, "HUNTERSLODGE" },
        { CraftingBuildingName.MagicItems, "MAGICITEMS" },
        { CraftingBuildingName.Alchemist, "ALCHEMIST" },
        { CraftingBuildingName.Cook, "COOK" },
        { CraftingBuildingName.ToolMaker, "TOOLMAKER" },
        { CraftingBuildingName.FarmingMerchant, "FARMING_MERCHANT" }
    };

    public static string GetEnumString(CraftingBuildingName craftingBuildingName)
    {
        return CraftingBuildingNames.TryGetValue(craftingBuildingName, out string result) ? result : string.Empty;
    }

    public static bool DoesCraftingBuildingNameFit(string input, IReadOnlyCollection<CraftingBuildingName> craftingBuildingNamesToCheck)
    {
        if (string.IsNullOrEmpty(input))
        {
            return false;
        }

        foreach (var craftingBuildingName in craftingBuildingNamesToCheck)
        {
            if (CraftingBuildingNames.TryGetValue(craftingBuildingName, out string name)
                && input.Contains(name, System.StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}