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

namespace StatisticsAnalysisTool.Common
{
    public static class CraftingController
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

        private static IAsyncEnumerable<SimpleItemData> _simpleItemData = new List<SimpleItemData>().ToAsyncEnumerable();
        private static IAsyncEnumerable<ItemSpriteToJournalStruct> _craftingJournalData = new List<ItemSpriteToJournalStruct>().ToAsyncEnumerable();

        public static double GetRequiredJournalAmount(Item item, double ItemQuantityToBeCrafted, int level)
        {
            if (ItemQuantityToBeCrafted == 0)
            {
                return 0;
            }

            var totalBaseFame = GetTotalBaseFame(item.FullItemInformation.CraftingRequirements.TotalAmountResources, (ItemTier)item.Tier, (ItemLevel)level);
            var totalJournalFame = totalBaseFame * ItemQuantityToBeCrafted;
            return totalJournalFame / MaxJournalFame((ItemTier)item.Tier);
        }

        private static async ValueTask<int> GetSimpleItemItemValueAsync(string uniqueName)
        {
            var data = await _simpleItemData.FirstOrDefaultAsync(x => x.UniqueName == uniqueName).ConfigureAwait(false);
            return data?.ItemValue ?? 0;
        }

        public static async Task<Item> GetCraftingJournalItemAsync(int tier, string itemSpriteName)
        {
            var data = await _craftingJournalData.FirstOrDefaultAsync(x => x.Name == itemSpriteName).ConfigureAwait(false);
            return data.Id switch
            {
                CraftingJournalType.JournalMage => ItemController.GetItemByUniqueName($"T{tier}_JOURNAL_MAGE_EMPTY"),
                CraftingJournalType.JournalHunter => ItemController.GetItemByUniqueName($"T{tier}_JOURNAL_HUNTER_EMPTY"),
                CraftingJournalType.JournalWarrior => ItemController.GetItemByUniqueName($"T{tier}_JOURNAL_WARRIOR_EMPTY"),
                CraftingJournalType.JournalToolMaker => ItemController.GetItemByUniqueName($"T{tier}_JOURNAL_TOOLMAKER_EMPTY"),
                _ => null
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

        public static async Task<double> GetSetupFeeAsync(Item item, string artifactUniqueName, double essentialCraftingTax)
        {
            try
            {
                return await GetItemValueAsync(item.FullItemInformation.CraftingRequirements.TotalAmountResources, (ItemTier)item.Tier, (ItemLevel)item.Level, artifactUniqueName) / 20 * essentialCraftingTax;
            }
            catch (Exception e)
            {
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                return 0;
            }
        }

        public static async Task<double> GetItemValueAsync(int numberOfMaterials, ItemTier tier, ItemLevel level, string artifactUniqueName)
        {
            var artifactItemValue = await GetSimpleItemItemValueAsync(artifactUniqueName);
            return (tier, level) switch
            {
                (ItemTier.T2, ItemLevel.Level0) => numberOfMaterials * 2,
                (ItemTier.T3, ItemLevel.Level0) => numberOfMaterials * 6,
                (ItemTier.T4, ItemLevel.Level0) => (numberOfMaterials * 14) + artifactItemValue,
                (ItemTier.T4, ItemLevel.Level1) => (numberOfMaterials * 30) + artifactItemValue,
                (ItemTier.T4, ItemLevel.Level2) => (numberOfMaterials * 54) + artifactItemValue,
                (ItemTier.T4, ItemLevel.Level3) => (numberOfMaterials * 102) + artifactItemValue,
                (ItemTier.T5, ItemLevel.Level0) => (numberOfMaterials * 30) + artifactItemValue,
                (ItemTier.T5, ItemLevel.Level1) => (numberOfMaterials * 62) + artifactItemValue,
                (ItemTier.T5, ItemLevel.Level2) => (numberOfMaterials * 118) + artifactItemValue,
                (ItemTier.T5, ItemLevel.Level3) => (numberOfMaterials * 230) + artifactItemValue,
                (ItemTier.T6, ItemLevel.Level0) => (numberOfMaterials * 62) + artifactItemValue,
                (ItemTier.T6, ItemLevel.Level1) => (numberOfMaterials * 126) + artifactItemValue,
                (ItemTier.T6, ItemLevel.Level2) => (numberOfMaterials * 246) + artifactItemValue,
                (ItemTier.T6, ItemLevel.Level3) => (numberOfMaterials * 486) + artifactItemValue,
                (ItemTier.T7, ItemLevel.Level0) => (numberOfMaterials * 126) + artifactItemValue,
                (ItemTier.T7, ItemLevel.Level1) => (numberOfMaterials * 254) + artifactItemValue,
                (ItemTier.T7, ItemLevel.Level2) => (numberOfMaterials * 502) + artifactItemValue,
                (ItemTier.T7, ItemLevel.Level3) => (numberOfMaterials * 1123) + artifactItemValue,
                (ItemTier.T8, ItemLevel.Level0) => (numberOfMaterials * 254) + artifactItemValue,
                (ItemTier.T8, ItemLevel.Level1) => (numberOfMaterials * 510) + artifactItemValue,
                (ItemTier.T8, ItemLevel.Level2) => (numberOfMaterials * 1014) + artifactItemValue,
                (ItemTier.T8, ItemLevel.Level3) => (numberOfMaterials * 2022) + artifactItemValue,
                _ => 0
            };
        }

        public static async Task<bool> LoadAsync()
        {
            _simpleItemData = await GetSimpleItemDataFromLocalAsync();
            _craftingJournalData = await GetJournalNameFromLocalAsync();

            if (_simpleItemData != null && await _simpleItemData.CountAsync() <= 0 || _craftingJournalData != null && await _craftingJournalData.CountAsync() <= 0)
            {
                Log.Warn($"{nameof(LoadAsync)}: No Simple item data found.");
                return false;
            }

            return true;
        }

        private static async Task<IAsyncEnumerable<SimpleItemData>> GetSimpleItemDataFromLocalAsync()
        {
            try
            {
                var options = new JsonSerializerOptions()
                {
                    ReadCommentHandling = JsonCommentHandling.Skip,
                    NumberHandling = JsonNumberHandling.AllowReadingFromString
                };

                var localItemString = await File.ReadAllTextAsync(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.GameFilesDirectoryName, Settings.Default.ItemsFileName), Encoding.UTF8);
                return (JsonSerializer.Deserialize<List<SimpleItemData>>(localItemString, options) ?? new List<SimpleItemData>()).ToAsyncEnumerable();
            }
            catch (Exception e)
            {
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                return new List<SimpleItemData>().ToAsyncEnumerable();
            }
        }

        public static async Task<IAsyncEnumerable<ItemSpriteToJournalStruct>> GetJournalNameFromLocalAsync()
        {
            try
            {
                var options = new JsonSerializerOptions()
                {
                    ReadCommentHandling = JsonCommentHandling.Skip,
                    NumberHandling = JsonNumberHandling.AllowReadingFromString
                };

                var localItemString = await File.ReadAllTextAsync(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.GameFilesDirectoryName, Settings.Default.ItemSpriteToJournalFileName), Encoding.UTF8);
                return (JsonSerializer.Deserialize<List<ItemSpriteToJournalStruct>>(localItemString, options) ?? new List<ItemSpriteToJournalStruct>()).ToAsyncEnumerable();
            }
            catch (Exception e)
            {
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                return new List<ItemSpriteToJournalStruct>().ToAsyncEnumerable();
            }
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