using log4net;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Models.ItemWindowModel;
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

        public static async Task<ItemType> GetItemTypeAsync(int index)
        {
            var item = Items?.FirstOrDefault(i => i.Index == index);

            if (item != null)
            {
                // TODO: Need rework
                //var fullItemInfo = await GetFullItemInformationAsync(item);
                //item.FullItemInformation = fullItemInfo;
            }

            //var itemType = !string.IsNullOrEmpty(item?.FullItemInformation?.ItemType) ? item.FullItemInformation?.ItemType : "UNKNOWN";

            return ItemType.Unknown;

            //return itemType.ToUpper() switch
            //{
            //    "WEAPON" => ItemType.Weapon,
            //    "EQUIPMENT" => ItemType.Equipment,
            //    "SIMPLE" => ItemType.Simple,
            //    "FARMABLE" => ItemType.Farmable,
            //    "CONSUMABLE" => ItemType.Consumable,
            //    "CONSUMABLEFROMINVENTORY" => ItemType.ConsumableFromInventory,
            //    "JOURNAL" => ItemType.Journal,
            //    "LABOURERCONTRACT" => ItemType.LabourerContract,
            //    "FURNITURE" => ItemType.Furniture,
            //    _ => ItemType.Unknown
            //};
        }

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

            // TODO: Rework
            //if (item.FullItemInformation?.Tier != null)
            //{
            //    return item.FullItemInformation.Tier;
            //}

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

        public static ItemJsonObject GetSpecificItemInfo(string uniqueName)
        {
            if (!IsItemsJsonLoaded())
            {
                return null;
            }

            if (ItemsJson.Items.HideoutItem.UniqueName == uniqueName)
            {
                return ItemsJson.Items.HideoutItem;
            }

            foreach (var simpleItem in ItemsJson.Items.FarmableItem.Where(simpleItem => simpleItem.UniqueName == uniqueName))
            {
                return simpleItem;
            }

            foreach (var simpleItem in ItemsJson.Items.SimpleItem.Where(simpleItem => simpleItem.UniqueName == uniqueName))
            {
                return simpleItem;
            }

            foreach (var simpleItem in ItemsJson.Items.ConsumableItem.Where(simpleItem => simpleItem.UniqueName == uniqueName))
            {
                return simpleItem;
            }

            foreach (var simpleItem in ItemsJson.Items.ConsumableFromInventoryItem.Where(simpleItem => simpleItem.UniqueName == uniqueName))
            {
                return simpleItem;
            }

            foreach (var simpleItem in ItemsJson.Items.EquipmentItem.Where(simpleItem => simpleItem.UniqueName == uniqueName))
            {
                return simpleItem;
            }

            foreach (var simpleItem in ItemsJson.Items.Weapon.Where(simpleItem => simpleItem.UniqueName == uniqueName))
            {
                return simpleItem;
            }

            foreach (var simpleItem in ItemsJson.Items.Mount.Where(simpleItem => simpleItem.UniqueName == uniqueName))
            {
                return simpleItem;
            }

            foreach (var simpleItem in ItemsJson.Items.FurnitureItem.Where(simpleItem => simpleItem.UniqueName == uniqueName))
            {
                return simpleItem;
            }

            foreach (var simpleItem in ItemsJson.Items.JournalItem.Where(simpleItem => simpleItem.UniqueName == uniqueName))
            {
                return simpleItem;
            }

            foreach (var simpleItem in ItemsJson.Items.LabourerContract.Where(simpleItem => simpleItem.UniqueName == uniqueName))
            {
                return simpleItem;
            }

            foreach (var simpleItem in ItemsJson.Items.MountSkin.Where(simpleItem => simpleItem.UniqueName == uniqueName))
            {
                return simpleItem;
            }

            foreach (var simpleItem in ItemsJson.Items.CrystalLeagueItem.Where(simpleItem => simpleItem.UniqueName == uniqueName))
            {
                return simpleItem;
            }

            return null;
        }

        [Obsolete("Must be rebuilt because ItemInfo no longer exists.")]
        public static bool IsItemSlotType(ItemInformation itemInfo, string slotType)
        {
            return itemInfo?.SlotType == slotType;
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
                    }
                    return ItemsJson?.Items != null;
                }

                ItemsJson = await GetItemsJsonFromLocal();
                return ItemsJson?.Items != null;
            }

            if (await GetItemsJsonFromWebAsync(url))
            {
                ItemsJson = await GetItemsJsonFromLocal();
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
                                     | JsonNumberHandling.WriteAsString
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

        [Obsolete]
        private static string GetItemListSourceUrlIfExist()
        {
            var url = SettingsController.CurrentSettings.ItemListSourceUrl ?? string.Empty;

            if (string.IsNullOrEmpty(url))
            {
                url = Settings.Default.DefaultItemListSourceUrl ?? string.Empty;

                if (!string.IsNullOrEmpty(url))
                {
                    SettingsController.CurrentSettings.ItemListSourceUrl = Settings.Default.DefaultItemListSourceUrl;
                    _ = MessageBox.Show(LanguageController.Translation("DEFAULT_ITEMLIST_HAS_BEEN_LOADED"), LanguageController.Translation("NOTE"));
                }
            }

            return url;
        }

        #endregion
    }
}