using log4net;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using StatisticsAnalysisTool.Models.ItemsJsonModel;

namespace StatisticsAnalysisTool.Common
{
    public static class CraftingController
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);
        
        public static double GetRequiredJournalAmount(Item item, double itemQuantityToBeCrafted)
        {
            if (itemQuantityToBeCrafted == 0)
            {
                return 0;
            }

            // TODO: Rework
            //var totalBaseFame = GetTotalBaseFame(item.FullItemInformation.CraftingRequirements.TotalAmountResources, (ItemTier)item.Tier, (ItemLevel)item.Level);
            //var totalJournalFame = totalBaseFame * itemQuantityToBeCrafted;
            //return totalJournalFame / MaxJournalFame((ItemTier)item.Tier);
            return 0;
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
                
                if (uniqueName.Contains("_CAPE")
                    || uniqueName.Contains("_BAG") 
                    || uniqueName.Contains("_BAG_INSIGHT"))
                {
                    return CraftingJournalType.JournalToolMaker;
                }
            }

            return craftingCategory switch
            {
                "tools" or "gatherergear" => CraftingJournalType.JournalToolMaker,
                "cloth_helmet" or "cloth_armor" or "cloth_shoes" or "cursestaff" or "firestaff" or "froststaff" or "arcanestaff" or "holystaff" => CraftingJournalType.JournalMage,
                "plate_helmet" or "plate_armor" or "plate_shoes" or "crossbow" or "axe" or "sword" or "hammer" or "mace" or "knuckles" => CraftingJournalType.JournalWarrior,
                "leather_helmet" or "leather_armor" or "leather_shoes" or "bow" or "naturestaff" or "dagger" or "spear" or "quarterstaff" => CraftingJournalType.JournalHunter,
                _ => CraftingJournalType.Unknown
            };
        }

        private static int MaxJournalFame(ItemTier tier)
        {
            return tier switch
            {
                ItemTier.T2 => (int)CraftingJournalFame.T2,
                ItemTier.T3 => (int)CraftingJournalFame.T3,
                ItemTier.T4 => (int)CraftingJournalFame.T4,
                ItemTier.T5 => (int)CraftingJournalFame.T5,
                ItemTier.T6 => (int)CraftingJournalFame.T6,
                ItemTier.T7 => (int)CraftingJournalFame.T7,
                ItemTier.T8 => (int)CraftingJournalFame.T8,
                _ => 0
            };
        }

        public static double GetTotalBaseFame(int numberOfMaterials, ItemTier tier, ItemLevel level)
        {
            return (tier, level) switch
            {
                (ItemTier.T2, ItemLevel.Level0) => numberOfMaterials * 1.5,
                (ItemTier.T3, ItemLevel.Level0) => numberOfMaterials * 7.5,
                (ItemTier.T4, ItemLevel.Level0) => numberOfMaterials * 22.5,
                (ItemTier.T4, ItemLevel.Level1) => numberOfMaterials * 37.5,
                (ItemTier.T4, ItemLevel.Level2) => numberOfMaterials * 52.5,
                (ItemTier.T4, ItemLevel.Level3) => numberOfMaterials * 67.5,
                (ItemTier.T5, ItemLevel.Level0) => numberOfMaterials * 90,
                (ItemTier.T5, ItemLevel.Level1) => numberOfMaterials * 172.5,
                (ItemTier.T5, ItemLevel.Level2) => numberOfMaterials * 255,
                (ItemTier.T5, ItemLevel.Level3) => numberOfMaterials * 337.5,
                (ItemTier.T6, ItemLevel.Level0) => numberOfMaterials * 270,
                (ItemTier.T6, ItemLevel.Level1) => numberOfMaterials * 532.5,
                (ItemTier.T6, ItemLevel.Level2) => numberOfMaterials * 795,
                (ItemTier.T6, ItemLevel.Level3) => numberOfMaterials * 1057.5,
                (ItemTier.T7, ItemLevel.Level0) => numberOfMaterials * 645,
                (ItemTier.T7, ItemLevel.Level1) => numberOfMaterials * 1282.5,
                (ItemTier.T7, ItemLevel.Level2) => numberOfMaterials * 1920,
                (ItemTier.T7, ItemLevel.Level3) => numberOfMaterials * 2557.5,
                (ItemTier.T8, ItemLevel.Level0) => numberOfMaterials * 1395,
                (ItemTier.T8, ItemLevel.Level1) => numberOfMaterials * 2782.5,
                (ItemTier.T8, ItemLevel.Level2) => numberOfMaterials * 4170,
                (ItemTier.T8, ItemLevel.Level3) => numberOfMaterials * 5557.5,
                _ => 0
            };
        }

        public static double GetCraftingTax(int foodValue, Item item, int itemQuantity)
        {
            // TODO: Rework
            try
            {
                //return itemQuantity * GetSetupFeePerFoodConsumed(foodValue, item.FullItemInformation.CraftingRequirements.TotalAmountResources, 
                //    (ItemTier)item.Tier, (ItemLevel)item.Level, item.FullItemInformation.CraftingRequirements.CraftResourceList);
                return 0;
            }
            catch (Exception e)
            {
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                return 0;
            }
        }
        
        public static double GetSetupFeePerFoodConsumed(int foodValue, int numberOfMaterials, ItemTier tier, ItemLevel level, IEnumerable<CraftResource> craftRequiredResources)
        {
            var tierFactor = (tier, level) switch
            {
                (ItemTier.T2, ItemLevel.Level0) => 1,
                (ItemTier.T3, ItemLevel.Level0) => 1,
                (ItemTier.T4, ItemLevel.Level0) => 1.8,
                (ItemTier.T4, ItemLevel.Level1) => 3.6,
                (ItemTier.T4, ItemLevel.Level2) => 7.2,
                (ItemTier.T4, ItemLevel.Level3) => 14.4,
                (ItemTier.T5, ItemLevel.Level0) => 3.6,
                (ItemTier.T5, ItemLevel.Level1) => 7.2,
                (ItemTier.T5, ItemLevel.Level2) => 14.4,
                (ItemTier.T5, ItemLevel.Level3) => 28.8,
                (ItemTier.T6, ItemLevel.Level0) => 7.2,
                (ItemTier.T6, ItemLevel.Level1) => 14.4,
                (ItemTier.T6, ItemLevel.Level2) => 28.8,
                (ItemTier.T6, ItemLevel.Level3) => 57.6,
                (ItemTier.T7, ItemLevel.Level0) => 14.4,
                (ItemTier.T7, ItemLevel.Level1) => 28.8,
                (ItemTier.T7, ItemLevel.Level2) => 57.6,
                (ItemTier.T7, ItemLevel.Level3) => 115.2,
                (ItemTier.T8, ItemLevel.Level0) => 28.8,
                (ItemTier.T8, ItemLevel.Level1) => 57.6,
                (ItemTier.T8, ItemLevel.Level2) => 115.2,
                (ItemTier.T8, ItemLevel.Level3) => 230.4,
                _ => 1
            };

            var safeFoodValue = (foodValue <= 0) ? 1 : foodValue;
            return safeFoodValue / 100 * numberOfMaterials * (tierFactor + GetArtifactFactor(craftRequiredResources));
        }

        private static double GetArtifactFactor(IEnumerable<CraftResource> requiredResources, double craftingTaxDefault = 0.0)
        {
            var artifactResource = requiredResources.FirstOrDefault(x => x.UniqueName.Contains("ARTEFACT_TOKEN_FAVOR"));
            
            if (string.IsNullOrEmpty(artifactResource?.UniqueName) || !artifactResource.UniqueName.Contains("ARTEFACT_TOKEN_FAVOR"))
            {
                return craftingTaxDefault;
            }

            return artifactResource.UniqueName[..2] switch
            {
                "T4" when artifactResource.UniqueName[^1..] == "1" => 0.45f,
                "T4" when artifactResource.UniqueName[^1..] == "2" => 1.35f,
                "T4" when artifactResource.UniqueName[^1..] == "3" => 3.15f,
                "T4" when artifactResource.UniqueName[^1..] == "4" => 6.75f,
                "T5" when artifactResource.UniqueName[^1..] == "1" => 0.9f,
                "T5" when artifactResource.UniqueName[^1..] == "2" => 2.7f,
                "T5" when artifactResource.UniqueName[^1..] == "3" => 6.3f,
                "T5" when artifactResource.UniqueName[^1..] == "4" => 13.5f,
                "T6" when artifactResource.UniqueName[^1..] == "1" => 1.8f,
                "T6" when artifactResource.UniqueName[^1..] == "2" => 5.4f,
                "T6" when artifactResource.UniqueName[^1..] == "3" => 12.6f,
                "T6" when artifactResource.UniqueName[^1..] == "4" => 27.0f,
                "T7" when artifactResource.UniqueName[^1..] == "1" => 3.6f,
                "T7" when artifactResource.UniqueName[^1..] == "2" => 10.8f,
                "T7" when artifactResource.UniqueName[^1..] == "3" => 25.2f,
                "T7" when artifactResource.UniqueName[^1..] == "4" => 45.0f,
                "T8" when artifactResource.UniqueName[^1..] == "1" => 7.2f,
                "T8" when artifactResource.UniqueName[^1..] == "2" => 21.6f,
                "T8" when artifactResource.UniqueName[^1..] == "3" => 50.4f,
                "T8" when artifactResource.UniqueName[^1..] == "4" => 108.0f,
                _ => craftingTaxDefault
            };
        }
        
        #region Calculations

        public static double GetSetupFeeCalculation(int? craftingItemQuantity, double? setupFee, double? sellPricePerItem)
        {
            if (craftingItemQuantity != null && setupFee != null && sellPricePerItem != null && craftingItemQuantity > 0 && setupFee > 0 && sellPricePerItem > 0)
            {
                return (double)craftingItemQuantity * (double)sellPricePerItem / 100 * (double)setupFee;
            }

            return 0.0d;
        }

        #endregion

        public struct ItemSpriteToJournalStruct
        {
            public string Name { get; set; }
            public CraftingJournalType Id { get; set; }
        }
    }
}