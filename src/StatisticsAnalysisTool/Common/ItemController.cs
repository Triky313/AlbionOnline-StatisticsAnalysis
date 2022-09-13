using log4net;
using StatisticsAnalysisTool.Common.Converters;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Models.ItemsJsonModel;
using StatisticsAnalysisTool.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using JsonSerializer = System.Text.Json.JsonSerializer;
using Mount = StatisticsAnalysisTool.Models.ItemsJsonModel.Mount;

namespace StatisticsAnalysisTool.Common
{
    public static class ItemController
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

        public static ObservableCollection<Item> Items;
        public static ItemsJson ItemsJson;

        #region General

        public static string GetCleanUniqueName(string uniqueName)
        {
            var uniqueNameArray = uniqueName.Split("@");
            return uniqueNameArray[0];
        }

        public static ItemQuality GetQuality(int value)
        {
            return FrequentlyValues.ItemQualities.FirstOrDefault(x => x.Value == value).Key;
        }

        public static ulong GetMinPrice(List<ulong> list)
        {
            var min = ulong.MaxValue;
            foreach (var value in list)
            {
                if (value == 0)
                    continue;

                if (value < min)
                    min = value;
            }

            return min;
        }

        #endregion

        #region Item specific

        public static int GetItemLevel(string uniqueName)
        {
            if (uniqueName == null || !uniqueName.Contains('@'))
            {
                return 0;
            }

            return int.TryParse(uniqueName.Split('@')[1], out var number) ? number : 0;
        }

        public static int GetItemTier(Item item)
        {
            if (item?.UniqueName == null)
            {
                return -1;
            }

            var itemNameTierText = item.UniqueName?.Split('_')[0];
            if (itemNameTierText != null && itemNameTierText[..1] == "T" && int.TryParse(itemNameTierText.AsSpan(1, 1), out var result))
            {
                return result;
            }

            return -1;
        }

        #endregion

        #region Localized

        public static string LocalizedName(LocalizedNames localizedNames, string currentLanguage = null, string alternativeName = "NO_ITEM_NAME")
        {
            if (localizedNames == null)
            {
                return alternativeName;
            }

            if (string.IsNullOrEmpty(currentLanguage))
            {
                currentLanguage = LanguageController.CurrentCultureInfo?.TextInfo.CultureName.ToUpper();
            }

            return FrequentlyValues.GameLanguages
                    .FirstOrDefault(x => string.Equals(x.Value, currentLanguage, StringComparison.CurrentCultureIgnoreCase))
                    .Key switch
            {
                GameLanguage.UnitedStates => localizedNames.EnUs ?? alternativeName,
                GameLanguage.Germany => localizedNames.DeDe ?? alternativeName,
                GameLanguage.Russia => localizedNames.RuRu ?? alternativeName,
                GameLanguage.Poland => localizedNames.PlPl ?? alternativeName,
                GameLanguage.Brazil => localizedNames.PtBr ?? alternativeName,
                GameLanguage.France => localizedNames.FrFr ?? alternativeName,
                GameLanguage.Spain => localizedNames.EsEs ?? alternativeName,
                GameLanguage.Chinese => localizedNames.ZhCn ?? alternativeName,
                GameLanguage.Korean => localizedNames.KoKr ?? alternativeName,
                GameLanguage.Italy => localizedNames.ItIt ?? alternativeName,
                GameLanguage.Japan => localizedNames.JaJp ?? alternativeName,
                _ => alternativeName
            };
        }

        #endregion

        #region Items

        public static Item GetItemByIndex(int? index)
        {
            return index == null ? null : Items?.FirstOrDefault(i => i.Index == index);
        }

        public static Item GetItemByUniqueName(string uniqueName)
        {
            return Items?.FirstOrDefault(i => i.UniqueName == uniqueName) ?? Items?.FirstOrDefault(i => GetCleanUniqueName(i.UniqueName) == uniqueName);
        }

        public static bool IsTrash(int index)
        {
            var item = Items.FirstOrDefault(i => i.Index == index);
            return (item != null && item.UniqueName.Contains("TRASH")) || item == null;
        }

        public static async Task<bool> GetItemListFromJsonAsync()
        {
            var currentSettingsItemsJsonSourceUrl = SettingsController.CurrentSettings.ItemListSourceUrl;
            var url = GetSourceUrlOrDefault(Settings.Default.DefaultItemListSourceUrl, currentSettingsItemsJsonSourceUrl, ref currentSettingsItemsJsonSourceUrl);
            SettingsController.CurrentSettings.ItemListSourceUrl = currentSettingsItemsJsonSourceUrl;

            var localFilePath = $"{AppDomain.CurrentDomain.BaseDirectory}{Settings.Default.ItemListFileName}";

            if (string.IsNullOrEmpty(url))
            {
                return false;
            }

            if (File.Exists(localFilePath))
            {
                var fileDateTime = File.GetLastWriteTime(localFilePath);

                if (fileDateTime.AddDays(SettingsController.CurrentSettings.UpdateItemListByDays) < DateTime.Now)
                {
                    if (await GetItemListFromWebAsync(url))
                    {
                        Items = await GetItemListFromLocal();
                    }
                    return Items?.Count > 0;
                }

                Items = await GetItemListFromLocal();
                return Items?.Count > 0;
            }

            if (await GetItemListFromWebAsync(url))
            {
                Items = await GetItemListFromLocal();
            }
            return Items?.Count > 0;
        }

        private static async Task<ObservableCollection<Item>> GetItemListFromLocal()
        {
            try
            {
                var options = new JsonSerializerOptions()
                {
                    NumberHandling = JsonNumberHandling.AllowReadingFromString |
                                     JsonNumberHandling.WriteAsString
                };

                var localItemString = await File.ReadAllTextAsync($"{AppDomain.CurrentDomain.BaseDirectory}{Settings.Default.ItemListFileName}", Encoding.UTF8);
                return ConvertItemJsonObjectToItem(JsonSerializer.Deserialize<ObservableCollection<ItemListObject>>(localItemString, options));
            }
            catch
            {
                DeleteLocalFile($"{AppDomain.CurrentDomain.BaseDirectory}{Settings.Default.ItemListFileName}");
                return new ObservableCollection<Item>();
            }
        }

        private static ObservableCollection<Item> ConvertItemJsonObjectToItem(IEnumerable<ItemListObject> itemJsonObjectList)
        {
            var result = itemJsonObjectList.Select(item => new Item
            {
                LocalizationNameVariable = item.LocalizationNameVariable,
                LocalizationDescriptionVariable = item.LocalizationDescriptionVariable,
                LocalizedNames = item.LocalizedNames,
                Index = item.Index,
                UniqueName = item.UniqueName
            }).ToList();

            return new ObservableCollection<Item>(result);
        }

        private static async Task<bool> GetItemListFromWebAsync(string url)
        {
            using var client = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(600)
            };
            try
            {
                using var response = await client.GetAsync(url);
                using var content = response.Content;

                var fileString = await content.ReadAsStringAsync();
                await File.WriteAllTextAsync($"{AppDomain.CurrentDomain.BaseDirectory}{Settings.Default.ItemListFileName}", fileString, Encoding.UTF8);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static async Task SetFavoriteItemsFromLocalFileAsync()
        {
            var localFilePath = $"{AppDomain.CurrentDomain.BaseDirectory}{Settings.Default.FavoriteItemsFileName}";
            if (File.Exists(localFilePath))
            {
                try
                {
                    var localItemString = await File.ReadAllTextAsync(localFilePath, Encoding.UTF8);

                    var favoriteItemList = JsonSerializer.Deserialize<List<string>>(localItemString);

                    if (favoriteItemList != null)
                    {
                        foreach (var uniqueName in favoriteItemList)
                        {
                            var item = Items.FirstOrDefault(i => i.UniqueName == uniqueName);
                            if (item != null)
                            {
                                item.IsFavorite = true;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                    Log.Error(MethodBase.GetCurrentMethod()?.Name, e);
                }
            }
        }

        public static void SaveFavoriteItemsToLocalFile()
        {
            var localFilePath = $"{AppDomain.CurrentDomain.BaseDirectory}{Settings.Default.FavoriteItemsFileName}";
            var favoriteItems = Items?.Where(x => x.IsFavorite);
            var toSaveFavoriteItems = favoriteItems?.Select(x => x.UniqueName);
            var fileString = JsonSerializer.Serialize(toSaveFavoriteItems);

            try
            {
                File.WriteAllText(localFilePath, fileString, Encoding.UTF8);
            }
            catch (Exception e)
            {
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                Log.Error(MethodBase.GetCurrentMethod()?.Name, e);
            }
        }

        public static bool IsItemsLoaded()
        {
            return Items?.Count > 0;
        }

        #endregion Item list

        #region Item extra information

        public static async Task SetFullItemInfoToItems()
        {
            var tasks = await Items.ToAsyncEnumerable()
                .Select(item => Task.Run(async () =>
                {
                    item.FullItemInformation = await GetSpecificItemInfoAsync(item.UniqueName);
                    item.ShopCategory = GetShopCategory(item.UniqueName);
                    item.ShopShopSubCategory1 = GetShopSubCategory(item.UniqueName);
                }))
                .ToListAsync();

            await Task.WhenAll(tasks);
        }

        private static async Task<ItemJsonObject> GetSpecificItemInfoAsync(string uniqueName)
        {
            var cleanUniqueName = GetCleanUniqueName(uniqueName);

            if (!IsItemsJsonLoaded())
            {
                return null;
            }

            if (ItemsJson.Items.HideoutItem.UniqueName == cleanUniqueName)
            {
                return ItemsJson.Items.HideoutItem;
            }

            await foreach (var item in ItemsJson.Items.FarmableItem.Where(item => item.UniqueName == cleanUniqueName).ToAsyncEnumerable())
            {
                item.ItemType = ItemType.Farmable;
                return item;
            }

            await foreach (var item in ItemsJson.Items.SimpleItem.Where(item => item.UniqueName == cleanUniqueName).ToAsyncEnumerable())
            {
                item.ItemType = ItemType.Simple;
                return item;
            }

            await foreach (var item in ItemsJson.Items.ConsumableItem.Where(item => item.UniqueName == cleanUniqueName).ToAsyncEnumerable())
            {
                item.ItemType = ItemType.Consumable;
                return item;
            }

            await foreach (var item in ItemsJson.Items.ConsumableFromInventoryItem.Where(item => item.UniqueName == cleanUniqueName).ToAsyncEnumerable())
            {
                item.ItemType = ItemType.ConsumableFromInventory;
                return item;
            }

            await foreach (var item in ItemsJson.Items.EquipmentItem.Where(item => item.UniqueName == cleanUniqueName).ToAsyncEnumerable())
            {
                item.ItemType = ItemType.Equipment;
                return item;
            }

            await foreach (var item in ItemsJson.Items.Weapon.Where(item => item.UniqueName == cleanUniqueName).ToAsyncEnumerable())
            {
                item.ItemType = ItemType.Weapon;
                return item;
            }

            await foreach (var item in ItemsJson.Items.Mount.Where(item => item.UniqueName == cleanUniqueName).ToAsyncEnumerable())
            {
                item.ItemType = ItemType.Mount;
                return item;
            }

            await foreach (var item in ItemsJson.Items.FurnitureItem.Where(item => item.UniqueName == cleanUniqueName).ToAsyncEnumerable())
            {
                item.ItemType = ItemType.Furniture;
                return item;
            }

            await foreach (var item in ItemsJson.Items.JournalItem.Where(item => item.UniqueName == cleanUniqueName).ToAsyncEnumerable())
            {
                item.ItemType = ItemType.Journal;
                return item;
            }

            await foreach (var item in ItemsJson.Items.LabourerContract.Where(item => item.UniqueName == cleanUniqueName).ToAsyncEnumerable())
            {
                item.ItemType = ItemType.LabourerContract;
                return item;
            }

            await foreach (var item in ItemsJson.Items.MountSkin.Where(item => item.UniqueName == cleanUniqueName).ToAsyncEnumerable())
            {
                item.ItemType = ItemType.MountSkin;
                return item;
            }

            await foreach (var item in ItemsJson.Items.CrystalLeagueItem.Where(item => item.UniqueName == cleanUniqueName).ToAsyncEnumerable())
            {
                item.ItemType = ItemType.CrystalLeague;
                return item;
            }

            return null;
        }

        public static ShopCategory GetShopCategory(string uniqueName)
        {
            return GetItemByUniqueName(uniqueName)?.FullItemInformation switch
            {
                HideoutItem hideoutItem => CategoryController.ShopCategoryStringToCategory(hideoutItem.ShopCategory),
                FarmableItem farmableItem => CategoryController.ShopCategoryStringToCategory(farmableItem.ShopCategory),
                SimpleItem simpleItem => CategoryController.ShopCategoryStringToCategory(simpleItem.ShopCategory),
                ConsumableItem consumableItem => CategoryController.ShopCategoryStringToCategory(consumableItem.ShopCategory),
                ConsumableFromInventoryItem consumableFromInventoryItem => CategoryController.ShopCategoryStringToCategory(consumableFromInventoryItem.ShopCategory),
                EquipmentItem equipmentItem => CategoryController.ShopCategoryStringToCategory(equipmentItem.ShopCategory),
                Weapon weapon => CategoryController.ShopCategoryStringToCategory(weapon.ShopCategory),
                Mount mount => CategoryController.ShopCategoryStringToCategory(mount.ShopCategory),
                FurnitureItem furnitureItem => CategoryController.ShopCategoryStringToCategory(furnitureItem.ShopCategory),
                JournalItem journalItem => CategoryController.ShopCategoryStringToCategory(journalItem.ShopCategory),
                LabourerContract labourerContract => CategoryController.ShopCategoryStringToCategory(labourerContract.ShopCategory),
                CrystalLeagueItem crystalLeagueItem => CategoryController.ShopCategoryStringToCategory(crystalLeagueItem.ShopCategory),
                _ => ShopCategory.Unknown
            };
        }

        public static ShopSubCategory GetShopSubCategory(string uniqueName)
        {
            var item = GetItemByUniqueName(uniqueName)?.FullItemInformation;

            return item switch
            {
                HideoutItem hideoutItem => CategoryController.ShopSubCategoryStringToShopSubCategory(hideoutItem.ShopSubCategory1),
                FarmableItem farmableItem => CategoryController.ShopSubCategoryStringToShopSubCategory(farmableItem.ShopSubCategory1),
                SimpleItem simpleItem => CategoryController.ShopSubCategoryStringToShopSubCategory(simpleItem.ShopSubCategory1),
                ConsumableItem consumableItem => CategoryController.ShopSubCategoryStringToShopSubCategory(consumableItem.ShopSubCategory1),
                ConsumableFromInventoryItem consumableFromInventoryItem => CategoryController.ShopSubCategoryStringToShopSubCategory(consumableFromInventoryItem.ShopSubCategory1),
                EquipmentItem equipmentItem => CategoryController.ShopSubCategoryStringToShopSubCategory(equipmentItem.ShopSubCategory1),
                Weapon weapon => CategoryController.ShopSubCategoryStringToShopSubCategory(weapon.ShopSubCategory1),
                Mount mount => CategoryController.ShopSubCategoryStringToShopSubCategory(mount.ShopSubCategory1),
                FurnitureItem furnitureItem => CategoryController.ShopSubCategoryStringToShopSubCategory(furnitureItem.ShopSubCategory1),
                JournalItem journalItem => CategoryController.ShopSubCategoryStringToShopSubCategory(journalItem.ShopSubCategory1),
                LabourerContract labourerContract => CategoryController.ShopSubCategoryStringToShopSubCategory(labourerContract.ShopSubCategory1),
                CrystalLeagueItem crystalLeagueItem => CategoryController.ShopSubCategoryStringToShopSubCategory(crystalLeagueItem.ShopSubCategory1),
                _ => ShopSubCategory.Unknown
            };
        }

        public struct ItemTypeStruct
        {
            public ItemTypeStruct(string uniqueName, ItemType itemType)
            {
                UniqueName = uniqueName;
                ItemType = itemType;
            }

            public string UniqueName { get; }
            public ItemType ItemType { get; }
        }

        public static ItemType GetItemType(int index)
        {
            var itemObject = Items?.FirstOrDefault(i => i.Index == index);

            if (itemObject == null || ItemsJson?.Items == null)
            {
                return ItemType.Unknown;
            }

            var itemTypeStructs = new List<ItemTypeStruct> { new(ItemsJson.Items.HideoutItem.UniqueName, ItemType.Hideout) };
            itemTypeStructs.AddRange(ItemsJson.Items.FarmableItem.Select(x => new ItemTypeStruct(x.UniqueName, x.ItemType)));
            itemTypeStructs.AddRange(ItemsJson.Items.SimpleItem.Select(x => new ItemTypeStruct(x.UniqueName, x.ItemType)));
            itemTypeStructs.AddRange(ItemsJson.Items.ConsumableItem.Select(x => new ItemTypeStruct(x.UniqueName, x.ItemType)));
            itemTypeStructs.AddRange(ItemsJson.Items.ConsumableFromInventoryItem.Select(x => new ItemTypeStruct(x.UniqueName, x.ItemType)));
            itemTypeStructs.AddRange(ItemsJson.Items.EquipmentItem.Select(x => new ItemTypeStruct(x.UniqueName, x.ItemType)));
            itemTypeStructs.AddRange(ItemsJson.Items.Weapon.Select(x => new ItemTypeStruct(x.UniqueName, x.ItemType)));
            itemTypeStructs.AddRange(ItemsJson.Items.Mount.Select(x => new ItemTypeStruct(x.UniqueName, x.ItemType)));
            itemTypeStructs.AddRange(ItemsJson.Items.FurnitureItem.Select(x => new ItemTypeStruct(x.UniqueName, x.ItemType)));
            itemTypeStructs.AddRange(ItemsJson.Items.JournalItem.Select(x => new ItemTypeStruct(x.UniqueName, x.ItemType)));
            itemTypeStructs.AddRange(ItemsJson.Items.LabourerContract.Select(x => new ItemTypeStruct(x.UniqueName, x.ItemType)));
            itemTypeStructs.AddRange(ItemsJson.Items.MountSkin.Select(x => new ItemTypeStruct(x.UniqueName, x.ItemType)));
            itemTypeStructs.AddRange(ItemsJson.Items.CrystalLeagueItem.Select(x => new ItemTypeStruct(x.UniqueName, x.ItemType)));

            return itemTypeStructs.FirstOrDefault(x => x.UniqueName == itemObject.UniqueName).ItemType;
        }

        public static async Task<bool> GetItemsJsonAsync()
        {
            var currentSettingsItemsJsonSourceUrl = SettingsController.CurrentSettings.ItemsJsonSourceUrl;
            var url = GetSourceUrlOrDefault(Settings.Default.DefaultItemsJsonSourceUrl, currentSettingsItemsJsonSourceUrl, ref currentSettingsItemsJsonSourceUrl);
            SettingsController.CurrentSettings.ItemsJsonSourceUrl = currentSettingsItemsJsonSourceUrl;

            var localFilePath = $"{AppDomain.CurrentDomain.BaseDirectory}{Settings.Default.ItemsJsonFileName}";

            if (string.IsNullOrEmpty(url))
            {
                return false;
            }

            if (File.Exists(localFilePath))
            {
                var fileDateTime = File.GetLastWriteTime(localFilePath);

                if (fileDateTime.AddDays(SettingsController.CurrentSettings.UpdateItemsJsonByDays) < DateTime.Now)
                {
                    if (await GetItemsJsonFromWebAsync(url))
                    {
                        ItemsJson = await GetItemsJsonFromLocal();
                        await SetFullItemInfoToItems();
                    }

                    return ItemsJson?.Items != null;
                }

                ItemsJson = await GetItemsJsonFromLocal();
                await SetFullItemInfoToItems();

                return ItemsJson?.Items != null;
            }

            if (await GetItemsJsonFromWebAsync(url))
            {
                ItemsJson = await GetItemsJsonFromLocal();
                await SetFullItemInfoToItems();
            }

            return ItemsJson?.Items != null;
        }

        private static async Task<bool> GetItemsJsonFromWebAsync(string url)
        {
            using var client = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(600)
            };

            try
            {
                using var response = await client.GetAsync(url);
                using var content = response.Content;

                var fileString = await content.ReadAsStringAsync();
                await File.WriteAllTextAsync($"{AppDomain.CurrentDomain.BaseDirectory}{Settings.Default.ItemsJsonFileName}", fileString, Encoding.UTF8);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static async Task<ItemsJson> GetItemsJsonFromLocal()
        {
            var localFilePath = $"{AppDomain.CurrentDomain.BaseDirectory}{Settings.Default.ItemsJsonFileName}";

            try
            {
                var options = new JsonSerializerOptions()
                {
                    NumberHandling = JsonNumberHandling.AllowReadingFromString
                                     | JsonNumberHandling.WriteAsString,
                    Converters =
                    {
                        new CraftingRequirementsToCraftingRequirementsList(),
                        new BoolConverter()
                    }
                };

                var localFileString = await File.ReadAllTextAsync(localFilePath, Encoding.UTF8);
                return JsonSerializer.Deserialize<ItemsJson>(localFileString, options);
            }
            catch
            {
                DeleteLocalFile(localFilePath);
                return new ItemsJson();
            }
        }

        public static bool IsItemsJsonLoaded()
        {
            return ItemsJson?.Items != null;
        }

        #endregion

        #region Estimated market value

        public static void SetEstimatedMarketValue(string uniqueName, long estimatedMarketValueInternal, DateTime timestamp)
        {
            var item = GetItemByUniqueName(uniqueName);
            if (item == null)
            {
                return;
            }

            item.LastEstimatedMarketValueUpdate = timestamp;
            item.EstimatedMarketValue = FixPoint.FromInternalValue(estimatedMarketValueInternal);
        }

        #endregion

        #region Util methods

        private static void DeleteLocalFile(string localFileString)
        {
            if (File.Exists(localFileString))
            {
                try
                {
                    File.Delete(localFileString);
                }
                catch (Exception e)
                {
                    ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                    Log.Error(MethodBase.GetCurrentMethod()?.Name, e);
                }
            }
        }

        // ReSharper disable once RedundantAssignment
        private static string GetSourceUrlOrDefault(string defaultUrl, string sourceUrl, ref string newSourceUrl)
        {
            var tempSourceUrl = string.IsNullOrEmpty(sourceUrl) ? defaultUrl : sourceUrl;
            newSourceUrl = tempSourceUrl;
            return newSourceUrl;
        }

        #endregion
    }
}