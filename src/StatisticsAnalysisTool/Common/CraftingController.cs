using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Models.ItemsJsonModel;
using System.Collections.Generic;
using System.Linq;

namespace StatisticsAnalysisTool.Common;

public static class CraftingController
{
    public static int GetTotalAmountResources(List<CraftingRequirements> craftingRequirements)
    {
        var craftingRequirement = craftingRequirements.FirstOrDefault();
        if (craftingRequirement == null)
        {
            return 0;
        }

        return craftingRequirement.CraftResource
            .Where(x => !x.UniqueName.ToUpper().Contains("_ARTEFACT_") && !x.UniqueName.ToUpper().Contains("_FAVOR_") && !x.UniqueName.ToUpper().Contains("_ALCHEMY_"))
            .Sum(craftResource => craftResource.Count);
    }

    public static Item GetCraftingJournalItem(int tier, CraftingJournalType craftingJournalType)
    {
        return craftingJournalType switch
        {
            CraftingJournalType.JournalMage => ItemController.GetItemByUniqueName($"T{tier}_JOURNAL_MAGE_EMPTY"),
            CraftingJournalType.JournalHunter => ItemController.GetItemByUniqueName($"T{tier}_JOURNAL_HUNTER_EMPTY"),
            CraftingJournalType.JournalWarrior => ItemController.GetItemByUniqueName($"T{tier}_JOURNAL_WARRIOR_EMPTY"),
            CraftingJournalType.JournalToolMaker => ItemController.GetItemByUniqueName($"T{tier}_JOURNAL_TOOLMAKER_EMPTY"),
            _ => null
        };
    }

    public static CraftingJournalType GetCraftingJournalType(string uniqueName, string craftingCategory)
    {
        if (craftingCategory is "offhand" or null)
        {
            if (uniqueName.Contains("OFF_SCHIELD")
                || uniqueName.Contains("OFF_TOWERSHIELD")
                || uniqueName.Contains("OFF_SPIKEDSHIELD")
                || uniqueName.Contains("OFF_TOWERSHIELD"))
            {
                return CraftingJournalType.JournalWarrior;
            }

            if (uniqueName.Contains("OFF_JESTERCANE")
                || uniqueName.Contains("OFF_TORCH")
                || uniqueName.Contains("OFF_LAMP")
                || uniqueName.Contains("OFF_TALISMAN")
                || uniqueName.Contains("OFF_HORN"))
            {
                return CraftingJournalType.JournalHunter;
            }

            if (uniqueName.Contains("OFF_CENSER")
                || uniqueName.Contains("OFF_TOTEM")
                || uniqueName.Contains("OFF_DEMONSKULL")
                || uniqueName.Contains("OFF_ORB")
                || uniqueName.Contains("OFF_BOOK"))
            {
                return CraftingJournalType.JournalMage;
            }
        }

        if (uniqueName.Contains("_CAPE")
            || uniqueName.Contains("_BAG")
            || uniqueName.Contains("_BAG_INSIGHT"))
        {
            return CraftingJournalType.JournalToolMaker;
        }

        return craftingCategory switch
        {
            "tools" or "gatherergear" or "trackingtool" => CraftingJournalType.JournalToolMaker,
            "cloth_helmet" or "cloth_armor" or "cloth_shoes" or "cursestaff" or "firestaff" or "froststaff" or "arcanestaff" or "holystaff" => CraftingJournalType.JournalMage,
            "plate_helmet" or "plate_armor" or "plate_shoes" or "crossbow" or "axe" or "sword" or "hammer" or "mace" or "knuckles" => CraftingJournalType.JournalWarrior,
            "leather_helmet" or "leather_armor" or "leather_shoes" or "bow" or "naturestaff" or "dagger" or "spear" or "quarterstaff" or "shapeshifterstaff" => CraftingJournalType.JournalHunter,
            _ => CraftingJournalType.Unknown
        };
    }

    public static double GetTotalBaseFame(int numberOfMaterials, ItemTier tier, ItemLevel level)
    {
        return (tier, level) switch
        {
            (ItemTier.T2, ItemLevel.Level0) => numberOfMaterials * 1.5,
            (ItemTier.T3, ItemLevel.Level0) => numberOfMaterials * 7.5,
            (ItemTier.T4, ItemLevel.Level0) => numberOfMaterials * 22.5,
            (ItemTier.T4, ItemLevel.Level1) => numberOfMaterials * 45,
            (ItemTier.T4, ItemLevel.Level2) => numberOfMaterials * 90,
            (ItemTier.T4, ItemLevel.Level3) => numberOfMaterials * 180,
            (ItemTier.T4, ItemLevel.Level4) => numberOfMaterials * 360,
            (ItemTier.T5, ItemLevel.Level0) => numberOfMaterials * 90,
            (ItemTier.T5, ItemLevel.Level1) => numberOfMaterials * 180,
            (ItemTier.T5, ItemLevel.Level2) => numberOfMaterials * 360,
            (ItemTier.T5, ItemLevel.Level3) => numberOfMaterials * 720,
            (ItemTier.T5, ItemLevel.Level4) => numberOfMaterials * 1440,
            (ItemTier.T6, ItemLevel.Level0) => numberOfMaterials * 270,
            (ItemTier.T6, ItemLevel.Level1) => numberOfMaterials * 540,
            (ItemTier.T6, ItemLevel.Level2) => numberOfMaterials * 1080,
            (ItemTier.T6, ItemLevel.Level3) => numberOfMaterials * 2160,
            (ItemTier.T6, ItemLevel.Level4) => numberOfMaterials * 4320,
            (ItemTier.T7, ItemLevel.Level0) => numberOfMaterials * 645,
            (ItemTier.T7, ItemLevel.Level1) => numberOfMaterials * 1290,
            (ItemTier.T7, ItemLevel.Level2) => numberOfMaterials * 2580,
            (ItemTier.T7, ItemLevel.Level3) => numberOfMaterials * 5160,
            (ItemTier.T7, ItemLevel.Level4) => numberOfMaterials * 10320,
            (ItemTier.T8, ItemLevel.Level0) => numberOfMaterials * 1395,
            (ItemTier.T8, ItemLevel.Level1) => numberOfMaterials * 2790,
            (ItemTier.T8, ItemLevel.Level2) => numberOfMaterials * 5580,
            (ItemTier.T8, ItemLevel.Level3) => numberOfMaterials * 11160,
            (ItemTier.T8, ItemLevel.Level4) => numberOfMaterials * 22320,
            _ => 0
        };
    }
}