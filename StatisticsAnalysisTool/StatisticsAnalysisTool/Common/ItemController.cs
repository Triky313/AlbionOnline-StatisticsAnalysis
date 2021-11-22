using log4net;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace StatisticsAnalysisTool.Common
{
    public static class ItemController
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

        public static ObservableCollection<Item> Items;

        private static readonly string FullItemInformationFilePath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.FullItemInformationFileName);

        private static ObservableCollection<ItemInformation> _itemInformationList = new();

        public static readonly Brush ToggleOnColor = new SolidColorBrush((Color)Application.Current.Resources["Color.Blue.2"]);

        public static readonly Brush ToggleOffColor = new SolidColorBrush((Color)Application.Current.Resources["Color.Text.Normal"]);

        public static Item GetItemByIndex(int index)
        {
            return Items?.FirstOrDefault(i => i.Index == index);
        }

        public static Item GetItemByUniqueName(string uniqueName)
        {
            return Items?.FirstOrDefault(i => i.UniqueName == uniqueName);
        }

        public static string LocalizedName(LocalizedNames localizedNames, string currentLanguage = null, string alternativeName = "NO_ITEM_NAME")
        {
            if (localizedNames == null)
            {
                return alternativeName;
            }

            if (string.IsNullOrEmpty(currentLanguage))
            {
                currentLanguage = LanguageController.CurrentCultureInfo.TextInfo.CultureName.ToUpper();
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
                    _ => alternativeName
                };
        }

        public static bool IsTrash(int index)
        {
            var item = Items.FirstOrDefault(i => i.Index == index);
            return (item != null && item.UniqueName.Contains("TRASH")) || item == null;
        }

        public static async Task<ItemType> GetItemTypeAsync(int index)
        {
            var item = Items?.FirstOrDefault(i => i.Index == index);

            if (item != null)
            {
                var fullItemInfo = await GetFullItemInformationAsync(item);
                item.FullItemInformation = fullItemInfo;
            }

            var itemType = !string.IsNullOrEmpty(item?.FullItemInformation?.ItemType) ? item.FullItemInformation?.ItemType : "UNKNOWN";

            return itemType.ToUpper() switch
            {
                "WEAPON" => ItemType.Weapon,
                "EQUIPMENT" => ItemType.Equipment,
                "SIMPLE" => ItemType.Simple,
                "FARMABLE" => ItemType.Farmable,
                "CONSUMABLE" => ItemType.Consumable,
                "CONSUMABLEFROMINVENTORY" => ItemType.ConsumableFromInventory,
                "JOURNAL" => ItemType.Journal,
                "LABOURERCONTRACT" => ItemType.LabourerContract,
                "FURNITURE" => ItemType.Furniture,
                _ => ItemType.Unknown
            };
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

            if (item.FullItemInformation?.Tier != null)
            {
                return item.FullItemInformation.Tier;
            }

            return -1;
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

        #region Item list

        public static async Task<bool> GetItemListFromJsonAsync()
        {
            var url = GetItemListSourceUrlIfExist();
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
                return ConvertItemJsonObjectToItem(JsonSerializer.Deserialize<ObservableCollection<ItemJsonObject>>(localItemString, options));
            }
            catch
            {
                DeleteItemList();
                return new ObservableCollection<Item>();
            }
        }

        private static void DeleteItemList()
        {
            if (File.Exists($"{AppDomain.CurrentDomain.BaseDirectory}{Settings.Default.ItemListFileName}"))
            {
                try
                {
                    File.Delete($"{AppDomain.CurrentDomain.BaseDirectory}{Settings.Default.ItemListFileName}");
                }
                catch (Exception e)
                {
                    ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                    Log.Error(MethodBase.GetCurrentMethod()?.Name, e);
                }
            }
        }

        private static ObservableCollection<Item> ConvertItemJsonObjectToItem(ObservableCollection<ItemJsonObject> itemJsonObjectList)
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

        #endregion Item list

        #region ItemInformation

        public static bool IsFullItemInformationComplete =>
            Items?.All(item => IsItemInformationUpToDate(item?.FullItemInformation?.LastUpdate)) ?? false;

        public static async Task<ItemInformation> GetFullItemInformationAsync(Item item)
        {
            ItemInformation itemInformation;

            lock (_itemInformationList)
            {
                itemInformation = _itemInformationList.ToList().FirstOrDefault(x => x?.UniqueName == item?.UniqueName);
            }

            if (itemInformation?.HttpStatus == HttpStatusCode.NotFound)
            {
                return itemInformation;
            }

            if (string.IsNullOrEmpty(itemInformation?.UniqueName) || !IsItemInformationUpToDate(itemInformation.LastUpdate))
            {
                itemInformation = SetEssentialItemInformation(await ApiController.GetItemInfoFromJsonAsync(item?.UniqueName), item?.UniqueName);
                AddItemInformationToLocal(itemInformation);
            }

            return itemInformation;
        }

        private static ItemInformation SetEssentialItemInformation(ItemInformation itemInformation, string uniqueName)
        {
            if (itemInformation == null)
            {
                return null;
            }

            itemInformation.Level = GetItemLevel(uniqueName);
            itemInformation.UniqueName = uniqueName;
            return itemInformation;
        }

        private static void AddItemInformationToLocal(ItemInformation currentItemInformation)
        {
            if (currentItemInformation == null)
            {
                return;
            }

            lock (_itemInformationList)
            {
                var localItemInfo = _itemInformationList.ToList().FirstOrDefault(x => x?.UniqueName == currentItemInformation.UniqueName);
                _ = _itemInformationList.Remove(localItemInfo);

                currentItemInformation.LastUpdate = DateTime.Now;
                _itemInformationList.Add(currentItemInformation);
            }
        }

        private static bool IsItemInformationUpToDate(DateTime? lastUpdate)
        {
            if (lastUpdate == null || lastUpdate.Value.Year == 1)
            {
                return false;
            }

            var lastUpdateWithCycleDays = lastUpdate.Value.AddDays(SettingsController.CurrentSettings.FullItemInformationUpdateCycleDays);
            return lastUpdateWithCycleDays >= DateTime.UtcNow;
        }

        public static BitmapImage ExistFullItemInformationLocal(string uniqueName)
        {
            if (_itemInformationList.Any(x => x.UniqueName == uniqueName)
                && IsItemInformationUpToDate(_itemInformationList.FirstOrDefault(x => x.UniqueName == uniqueName)?.LastUpdate))
            {
                return new BitmapImage(new Uri(@"pack://application:,,,/Resources/check.png"));
            }

            if (_itemInformationList.Any(x => x.UniqueName == uniqueName)
                && !IsItemInformationUpToDate(_itemInformationList.FirstOrDefault(x => x.UniqueName == uniqueName)?.LastUpdate))
            {
                return new BitmapImage(new Uri(@"pack://application:,,,/Resources/outdated.png"));
            }

            return null;
        }

        public static void SaveItemInformationLocal()
        {
            var list = _itemInformationList;
            if (list == null)
            {
                return;
            }

            var itemInformationString = JsonSerializer.Serialize(list);

            using var writer = new StreamWriter(FullItemInformationFilePath);
            writer.Write(itemInformationString);
        }

        public static async Task GetItemInformationListFromLocalAsync()
        {
            await Task.Run(() =>
            {
                if (_itemInformationList is { Count: > 0 })
                {
                    return;
                }

                if (File.Exists(FullItemInformationFilePath))
                {
                    using var streamReader = new StreamReader(FullItemInformationFilePath, Encoding.UTF8);
                    var readContents = streamReader.ReadToEnd();
                    _itemInformationList = JsonSerializer.Deserialize<ObservableCollection<ItemInformation>>(readContents);
                }
                else
                {
                    _itemInformationList = new ObservableCollection<ItemInformation>();
                }

                SetItemInformationToItems(Items);
            });
        }

        private static void SetItemInformationToItems(ObservableCollection<Item> items)
        {
            if (items == null)
            {
                return;
            }

            foreach (var item in items.ToList())
            {
                var itemInformation = _itemInformationList.ToList().FirstOrDefault(x => x.UniqueName == item?.UniqueName);
                item.FullItemInformation = itemInformation;
            }
        }

        public static bool IsItemSlotType(ItemInformation itemInfo, string slotType)
        {
            return itemInfo?.SlotType == slotType;
        }

        #endregion ItemInformation
    }
}