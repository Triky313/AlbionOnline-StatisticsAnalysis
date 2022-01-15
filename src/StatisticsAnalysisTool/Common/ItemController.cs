using log4net;
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
using System.Windows;
using System.Windows.Media;
using StatisticsAnalysisTool.Common.Converters;
using JsonSerializer = System.Text.Json.JsonSerializer;
using Mount = StatisticsAnalysisTool.Models.ItemsJsonModel.Mount;

namespace StatisticsAnalysisTool.Common
{
    public static class ItemController
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

        public static ObservableCollection<Item> Items;
        public static ItemsJson ItemsJson;

        public static readonly Brush ToggleOnColor = new SolidColorBrush((Color)Application.Current.Resources["Color.Accent.Blue.2"]);

        public static readonly Brush ToggleOffColor = new SolidColorBrush((Color)Application.Current.Resources["Color.Text.1"]);

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

        public static Style LocationStyle(Location location)
        {
            return location switch
            {
                Location.Caerleon => Application.Current.FindResource("CaerleonStyle") as Style,
                Location.Thetford => Application.Current.FindResource("ThetfordStyle") as Style,
                Location.Bridgewatch => Application.Current.FindResource("BridgewatchStyle") as Style,
                Location.Martlock => Application.Current.FindResource("MartlockStyle") as Style,
                Location.Lymhurst => Application.Current.FindResource("LymhurstStyle") as Style,
                Location.FortSterling => Application.Current.FindResource("FortSterlingStyle") as Style,
                Location.ArthursRest => Application.Current.FindResource("ArthursRestStyle") as Style,
                Location.MerlynsRest => Application.Current.FindResource("MerlynsRestStyle") as Style,
                Location.MorganasRest => Application.Current.FindResource("MorganasRestStyle") as Style,
                Location.BlackMarket => Application.Current.FindResource("BlackMarketStyle") as Style,
                _ => Application.Current.FindResource("DefaultCityStyle") as Style
            };
        }

        public static Style GetStyleByTimestamp(DateTime value)
        {
            if (value.Date == DateTime.MinValue.Date)
            {
                return Application.Current.FindResource("ListView.Grid.Label.Date.NoValue") as Style;
            }

            if (value.AddHours(8) < DateTime.Now.ToUniversalTime().AddHours(-1))
            {
                return Application.Current.FindResource("ListView.Grid.Label.Date.ToOldFirst") as Style;
            }

            if (value.AddHours(4) < DateTime.Now.ToUniversalTime().AddHours(-1))
            {
                return Application.Current.FindResource("ListView.Grid.Label.Date.ToOldSecond") as Style;
            }

            if (value.AddHours(2) < DateTime.Now.ToUniversalTime().AddHours(-1))
            {
                return Application.Current.FindResource("ListView.Grid.Label.Date.ToOldThird") as Style;
            }

            return Application.Current.FindResource("ListView.Grid.Label.Date.Normal") as Style;
        }

        public static Style PriceStyle(bool bestSellMinPrice)
        {
            if (bestSellMinPrice)
            {
                return Application.Current.FindResource("ListView.Grid.StackPanel.Label.BestPrice") as Style;
            }

            return Application.Current.FindResource("ListView.Grid.StackPanel.Label.Price") as Style;
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
            if (uniqueName == null || !uniqueName.Contains("@"))
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

            var itemNameTierText = item.UniqueName.Split('_')[0];
            if (itemNameTierText[..1] == "T" && int.TryParse(itemNameTierText.Substring(1, 1), out var result))
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
                _ => alternativeName
            };
        }

        #endregion

        #region Items

        public static Item GetItemByIndex(int index)
        {
            return Items?.FirstOrDefault(i => i.Index == index);
        }

        public static Item GetItemByUniqueName(string uniqueName)
        {
            return Items?.FirstOrDefault(i => i.UniqueName == uniqueName);
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
                Timeout = TimeSpan.FromSeconds(300)
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
            await foreach (var item in Items.ToAsyncEnumerable())
            {
                item.FullItemInformation = await GetSpecificItemInfoAsync(item.UniqueName);
            }
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
                return item;
            }

            await foreach (var item in ItemsJson.Items.SimpleItem.Where(item => item.UniqueName == cleanUniqueName).ToAsyncEnumerable())
            {
                return item;
            }

            await foreach(var item in ItemsJson.Items.ConsumableItem.Where(item => item.UniqueName == cleanUniqueName).ToAsyncEnumerable())
            {
                return item;
            }

            await foreach(var item in ItemsJson.Items.ConsumableFromInventoryItem.Where(item => item.UniqueName == cleanUniqueName).ToAsyncEnumerable())
            {
                return item;
            }

            await foreach (var item in ItemsJson.Items.EquipmentItem.Where(item => item.UniqueName == cleanUniqueName).ToAsyncEnumerable())
            {
                return item;
            }

            await foreach (var item in ItemsJson.Items.Weapon.Where(item => item.UniqueName == cleanUniqueName).ToAsyncEnumerable())
            {
                return item;
            }

            await foreach (var item in ItemsJson.Items.Mount.Where(item => item.UniqueName == cleanUniqueName).ToAsyncEnumerable())
            {
                return item;
            }

            await foreach (var item in ItemsJson.Items.FurnitureItem.Where(item => item.UniqueName == cleanUniqueName).ToAsyncEnumerable())
            {
                return item;
            }

            await foreach (var item in ItemsJson.Items.JournalItem.Where(item => item.UniqueName == cleanUniqueName).ToAsyncEnumerable())
            {
                return item;
            }

            await foreach (var item in ItemsJson.Items.LabourerContract.Where(item => item.UniqueName == cleanUniqueName).ToAsyncEnumerable())
            {
                return item;
            }

            await foreach (var item in ItemsJson.Items.MountSkin.Where(item => item.UniqueName == cleanUniqueName).ToAsyncEnumerable())
            {
                return item;
            }

            await foreach (var item in ItemsJson.Items.CrystalLeagueItem.Where(item => item.UniqueName == cleanUniqueName).ToAsyncEnumerable())
            {
                return item;
            }

            return null;
        }

        public static async Task<Category> GetShopCategory(string uniqueName)
        {
            var item = await GetSpecificItemInfoAsync(uniqueName);

            return item switch
            {
                HideoutItem hideoutItem => CategoryController.ShopCategoryToCategory(hideoutItem.ShopCategory),
                FarmableItem farmableItem => CategoryController.ShopCategoryToCategory(farmableItem.ShopCategory),
                SimpleItem simpleItem => CategoryController.ShopCategoryToCategory(simpleItem.ShopCategory),
                ConsumableItem consumableItem => CategoryController.ShopCategoryToCategory(consumableItem.ShopCategory),
                ConsumableFromInventoryItem consumableFromInventoryItem => CategoryController.ShopCategoryToCategory(consumableFromInventoryItem.ShopCategory),
                EquipmentItem equipmentItem => CategoryController.ShopCategoryToCategory(equipmentItem.ShopCategory),
                Weapon weapon => CategoryController.ShopCategoryToCategory(weapon.ShopCategory),
                Mount mount => CategoryController.ShopCategoryToCategory(mount.ShopCategory),
                FurnitureItem furnitureItem => CategoryController.ShopCategoryToCategory(furnitureItem.ShopCategory),
                JournalItem journalItem => CategoryController.ShopCategoryToCategory(journalItem.ShopCategory),
                LabourerContract labourerContract => CategoryController.ShopCategoryToCategory(labourerContract.ShopCategory),
                CrystalLeagueItem crystalLeagueItem => CategoryController.ShopCategoryToCategory(crystalLeagueItem.ShopCategory),
                _ => Category.Unknown
            };
        }

        //public static ItemType GetShopSubCategory(string uniqueName)
        //{
        //    // TODO: Einbauen...
        //}

        public static ItemType GetItemType(string uniqueName)
        {
            return GetItemType(Items?.FirstOrDefault(x => x.UniqueName == uniqueName)?.Index ?? -1);
        }

        public static ItemType GetItemType(int index)
        {
            var itemObject = Items?.FirstOrDefault(i => i.Index == index);

            if (itemObject == null || ItemsJson?.Items == null)
            {
                return ItemType.Unknown;
            }

            if (ItemsJson.Items.HideoutItem?.UniqueName == itemObject.UniqueName)
            {
                return ItemType.Hideout;
            }

            if (ItemsJson.Items.FarmableItem.Any(item => item.UniqueName == itemObject.UniqueName))
            {
                return ItemType.Farmable;
            }

            if (ItemsJson.Items.SimpleItem.Any(item => item.UniqueName == itemObject.UniqueName))
            {
                return ItemType.Simple;
            }

            if (ItemsJson.Items.ConsumableItem.Any(item => item.UniqueName == itemObject.UniqueName))
            {
                return ItemType.Consumable;
            }

            if (ItemsJson.Items.ConsumableFromInventoryItem.Any(item => item.UniqueName == itemObject.UniqueName))
            {
                return ItemType.ConsumableFromInventory;
            }

            if (ItemsJson.Items.EquipmentItem.Any(item => item.UniqueName == itemObject.UniqueName))
            {
                return ItemType.Equipment;
            }

            if (ItemsJson.Items.Weapon.Any(item => item.UniqueName == itemObject.UniqueName))
            {
                return ItemType.Weapon;
            }

            if (ItemsJson.Items.Mount.Any(item => item.UniqueName == itemObject.UniqueName))
            {
                return ItemType.Mount;
            }

            if (ItemsJson.Items.FurnitureItem.Any(item => item.UniqueName == itemObject.UniqueName))
            {
                return ItemType.Furniture;
            }

            if (ItemsJson.Items.JournalItem.Any(simpleItem => simpleItem.UniqueName == itemObject.UniqueName))
            {
                return ItemType.Journal;
            }

            if (ItemsJson.Items.LabourerContract.Any(simpleItem => simpleItem.UniqueName == itemObject.UniqueName))
            {
                return ItemType.LabourerContract;
            }

            if (ItemsJson.Items.MountSkin.Any(simpleItem => simpleItem.UniqueName == itemObject.UniqueName))
            {
                return ItemType.MountSkin;
            }

            if (ItemsJson.Items.CrystalLeagueItem.Any(simpleItem => simpleItem.UniqueName == itemObject.UniqueName))
            {
                return ItemType.CrystalLeague;
            }

            return ItemType.Unknown;
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
                Timeout = TimeSpan.FromSeconds(300)
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