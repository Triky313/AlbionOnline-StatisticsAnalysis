using log4net;
using Newtonsoft.Json;
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
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace StatisticsAnalysisTool.Common
{
    public class ItemController
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static ObservableCollection<Item> Items;

        private static readonly string FullItemInformationFilePath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.FullItemInformationFileName);

        private static ObservableCollection<ItemInformation> _itemInformationList = new ObservableCollection<ItemInformation>();

        public static readonly Brush ToggleOnColor = new SolidColorBrush((Color) Application.Current.Resources["Color.Blue.2"]);

        public static readonly Brush ToggleOffColor = new SolidColorBrush((Color) Application.Current.Resources["Color.Text.Normal"]);

        public static Item GetItemByIndex(int index)
        {
            return Items.FirstOrDefault(i => i.Index == index);
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

            switch (FrequentlyValues.GameLanguages
                .FirstOrDefault(x => string.Equals(x.Value, currentLanguage, StringComparison.CurrentCultureIgnoreCase)).Key)
            {
                case GameLanguage.UnitedStates:
                    return localizedNames.EnUs ?? alternativeName;

                case GameLanguage.Germany:
                    return localizedNames.DeDe ?? alternativeName;

                case GameLanguage.Russia:
                    return localizedNames.RuRu ?? alternativeName;

                case GameLanguage.Poland:
                    return localizedNames.PlPl ?? alternativeName;

                case GameLanguage.Brazil:
                    return localizedNames.PtBr ?? alternativeName;

                case GameLanguage.France:
                    return localizedNames.FrFr ?? alternativeName;

                case GameLanguage.Spain:
                    return localizedNames.EsEs ?? alternativeName;

                case GameLanguage.Chinese:
                    return localizedNames.ZhCn ?? alternativeName;

                default:
                    return alternativeName;
            }
        }

        public static bool IsTrash(int index)
        {
            var item = Items.FirstOrDefault(i => i.Index == index);
            if (item != null && item.UniqueName.Contains("TRASH"))
            {
                return true;
            }

            return item == null;
        }
        
        public static ItemType GetItemType(int index)
        {
            var item = Items?.FirstOrDefault(i => i.Index == index);
            var itemType = !string.IsNullOrEmpty(item?.FullItemInformation?.ItemType) ? item.FullItemInformation?.ItemType : "UNKNOWN";
            switch (itemType)
            {
                case "WEAPON":
                    return ItemType.Weapon;
                case "EQUIPMENT":
                    return ItemType.Equipment;
                case "SIMPLE":
                    return ItemType.Simple;
                case "FARMABLE":
                    return ItemType.Farmable;
                case "CONSUMABLE":
                    return ItemType.Consumable;
                case "CONSUMABLEFROMINVENTORY":
                    return ItemType.ConsumableFromInventory;
                case "JOURNAL":
                    return ItemType.Journal;
                case "LABOURERCONTRACT":
                    return ItemType.LabourerContract;
                default:
                    return ItemType.Unknown;
            }
        }

        public static int GetItemLevel(string uniqueName)
        {
            if (uniqueName == null || !uniqueName.Contains("@")) return 0;

            return int.TryParse(uniqueName.Split('@')[1], out var number) ? number : 0;
        }
        
        public static int GetItemTier(Item item)
        {
            if (item?.UniqueName == null) return -1;

            var itemNameTierText = item.UniqueName.Split('_')[0];
            if (itemNameTierText.Substring(0, 1) == "T" && int.TryParse(itemNameTierText.Substring(1, 1), out var result)) return result;

            if (item.FullItemInformation?.Tier != null) return item.FullItemInformation.Tier;

            return -1;
        }

        public static ItemQuality GetQuality(int value)
        {
            return FrequentlyValues.ItemQualities.FirstOrDefault(x => x.Value == value).Key;
        }

        public static Style LocationStyle(Location location)
        {
            switch (location)
            {
                case Location.Caerleon:
                    return Application.Current.FindResource("CaerleonStyle") as Style;

                case Location.Thetford:
                    return Application.Current.FindResource("ThetfordStyle") as Style;

                case Location.Bridgewatch:
                    return Application.Current.FindResource("BridgewatchStyle") as Style;

                case Location.Martlock:
                    return Application.Current.FindResource("MartlockStyle") as Style;

                case Location.Lymhurst:
                    return Application.Current.FindResource("LymhurstStyle") as Style;

                case Location.FortSterling:
                    return Application.Current.FindResource("FortSterlingStyle") as Style;

                case Location.ArthursRest:
                    return Application.Current.FindResource("ArthursRestStyle") as Style;

                case Location.MerlynsRest:
                    return Application.Current.FindResource("MerlynsRestStyle") as Style;

                case Location.MorganasRest:
                    return Application.Current.FindResource("MorganasRestStyle") as Style;

                default:
                    return Application.Current.FindResource("DefaultCityStyle") as Style;
            }
        }

        public static Style GetStyleByTimestamp(DateTime value)
        {
            if (value.Date == DateTime.MinValue.Date)
                return Application.Current.FindResource("ListView.Grid.Label.Date.NoValue") as Style;

            if (value.AddHours(8) < DateTime.Now.ToUniversalTime().AddHours(-1))
                return Application.Current.FindResource("ListView.Grid.Label.Date.ToOldFirst") as Style;

            if (value.AddHours(4) < DateTime.Now.ToUniversalTime().AddHours(-1))
                return Application.Current.FindResource("ListView.Grid.Label.Date.ToOldSecond") as Style;

            if (value.AddHours(2) < DateTime.Now.ToUniversalTime().AddHours(-1))
                return Application.Current.FindResource("ListView.Grid.Label.Date.ToOldThird") as Style;

            return Application.Current.FindResource("ListView.Grid.Label.Date.Normal") as Style;
        }

        public static Style PriceStyle(bool bestSellMinPrice)
        {
            if (bestSellMinPrice) return Application.Current.FindResource("ListView.Grid.StackPanel.Label.BestPrice") as Style;

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
            var url = Settings.Default.ItemListSourceUrl;
            var localFilePath = $"{AppDomain.CurrentDomain.BaseDirectory}{Settings.Default.ItemListFileName}";

            if (!GetItemListSourceUrlIfExist(ref url))
            {
                return false;
            }

            if (File.Exists(localFilePath))
            {
                var fileDateTime = File.GetLastWriteTime(localFilePath);

                if (fileDateTime.AddDays(Settings.Default.UpdateItemListByDays) < DateTime.Now)
                {
                    if (await GetItemListFromWebAsync(url)) Items = GetItemListFromLocal();
                    return Items?.Count > 0;
                }

                Items = GetItemListFromLocal();
                return Items?.Count > 0;
            }

            if (await GetItemListFromWebAsync(url)) Items = GetItemListFromLocal();
            return Items?.Count > 0;
        }

        private static bool GetItemListSourceUrlIfExist(ref string url)
        {
            if (string.IsNullOrEmpty(Settings.Default.ItemListSourceUrl))
            {
                url = Settings.Default.DefaultItemListSourceUrl;
                if (string.IsNullOrEmpty(url))
                {
                    return false;
                }

                Settings.Default.ItemListSourceUrl = Settings.Default.DefaultItemListSourceUrl;
                MessageBox.Show(LanguageController.Translation("DEFAULT_ITEMLIST_HAS_BEEN_LOADED"), LanguageController.Translation("NOTE"));
            }

            return true;
        }

        private static ObservableCollection<Item> GetItemListFromLocal()
        {
            try
            {
                var localItemString = File.ReadAllText($"{AppDomain.CurrentDomain.BaseDirectory}{Settings.Default.ItemListFileName}", Encoding.UTF8);
                return ConvertItemJsonObjectToItem(JsonConvert.DeserializeObject<ObservableCollection<ItemJsonObject>>(localItemString));
            }
            catch
            {
                return new ObservableCollection<Item>();
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
                Timeout = TimeSpan.FromSeconds(90)
            };
            try
            {
                using var response = await client.GetAsync(url);
                using var content = response.Content;

                var fileString = await content.ReadAsStringAsync();
                File.WriteAllText($"{AppDomain.CurrentDomain.BaseDirectory}{Settings.Default.ItemListFileName}", fileString, Encoding.UTF8);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void SetFavoriteItemsFromLocalFile()
        {
            var localFilePath = $"{AppDomain.CurrentDomain.BaseDirectory}{Settings.Default.FavoriteItemsFileName}";
            if (File.Exists(localFilePath))
            {
                try
                {
                    var localItemString = File.ReadAllText(localFilePath, Encoding.UTF8);
                    foreach (var uniqueName in JsonConvert.DeserializeObject<List<string>>(localItemString))
                    {
                        var item = Items.FirstOrDefault(i => i.UniqueName == uniqueName);
                        if (item != null)
                        {
                            item.IsFavorite = true;
                        }
                    }
                }
                catch (Exception e)
                {
                    ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod().DeclaringType, e);
                    Log.Error(MethodBase.GetCurrentMethod().Name, e);
                }
            }
        }

        public static void SaveFavoriteItemsToLocalFile()
        {
            var localFilePath = $"{AppDomain.CurrentDomain.BaseDirectory}{Settings.Default.FavoriteItemsFileName}";
            var favoriteItems = Items?.Where(x => x.IsFavorite);
            var toSaveFavoriteItems = favoriteItems?.Select(x => x.UniqueName);
            var fileString = JsonConvert.SerializeObject(toSaveFavoriteItems);

            try
            {
                File.WriteAllText(localFilePath, fileString, Encoding.UTF8);
            }
            catch (Exception e)
            {
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod().DeclaringType, e);
                Log.Error(MethodBase.GetCurrentMethod().Name, e);
            }
        }

        #endregion Item list

        #region ItemInformation

        public static bool IsFullItemInformationComplete =>
            Items?.All(item => IsItemInformationUpToDate(item?.FullItemInformation?.LastUpdate)) ?? false;

        public static async Task<ItemInformation> GetFullItemInformationAsync(Item item)
        {
            var itemInformation = _itemInformationList.FirstOrDefault(x => x.UniqueName == item?.UniqueName);

            if (itemInformation?.HttpStatus == HttpStatusCode.NotFound) return itemInformation;

            if (string.IsNullOrEmpty(itemInformation?.UniqueName) || !IsItemInformationUpToDate(itemInformation.LastUpdate))
            {
                itemInformation = SetEssentialItemInformation(await ApiController.GetItemInfoFromJsonAsync(item?.UniqueName), item?.UniqueName);
                AddItemInformationToLocal(itemInformation);
            }

            return itemInformation;
        }

        private static ItemInformation SetEssentialItemInformation(ItemInformation itemInformation, string uniqueName)
        {
            if (itemInformation == null) return null;

            itemInformation.Level = GetItemLevel(uniqueName);
            itemInformation.UniqueName = uniqueName;
            return itemInformation;
        }

        private static void AddItemInformationToLocal(ItemInformation currentItemInformation)
        {
            if (currentItemInformation == null) return;

            var localItemInfo = _itemInformationList.FirstOrDefault(x => x.UniqueName == currentItemInformation.UniqueName);
            _itemInformationList.Remove(localItemInfo);

            currentItemInformation.LastUpdate = DateTime.Now;
            _itemInformationList.Add(currentItemInformation);
        }

        private static bool IsItemInformationUpToDate(DateTime? lastUpdate)
        {
            if (lastUpdate == null || lastUpdate.Value.Year == 1) return false;

            var lastUpdateWithCycleDays = lastUpdate.Value.AddDays(Settings.Default.FullItemInformationUpdateCycleDays);
            return lastUpdateWithCycleDays >= DateTime.UtcNow;
        }

        public static BitmapImage ExistFullItemInformationLocal(string uniqueName)
        {
            if (_itemInformationList.Any(x => x.UniqueName == uniqueName)
                && IsItemInformationUpToDate(_itemInformationList.FirstOrDefault(x => x.UniqueName == uniqueName)?.LastUpdate))
                return new BitmapImage(new Uri(@"pack://application:,,,/Resources/check.png"));

            if (_itemInformationList.Any(x => x.UniqueName == uniqueName)
                && !IsItemInformationUpToDate(_itemInformationList.FirstOrDefault(x => x.UniqueName == uniqueName)?.LastUpdate))
                return new BitmapImage(new Uri(@"pack://application:,,,/Resources/outdated.png"));

            return null;
        }

        public static void SaveItemInformationLocal()
        {
            var list = _itemInformationList;
            if (list == null) return;

            var itemInformationString = JsonConvert.SerializeObject(list);

            using (var writer = new StreamWriter(FullItemInformationFilePath))
            {
                writer.Write(itemInformationString);
            }
        }

        public static async Task GetItemInformationListFromLocalAsync()
        {
            await Task.Run(() =>
            {
                if (_itemInformationList != null && _itemInformationList.Count > 0) return;

                if (File.Exists(FullItemInformationFilePath))
                    using (var streamReader = new StreamReader(FullItemInformationFilePath, Encoding.UTF8))
                    {
                        var readContents = streamReader.ReadToEnd();
                        _itemInformationList = JsonConvert.DeserializeObject<ObservableCollection<ItemInformation>>(readContents);
                    }
                else
                    _itemInformationList = new ObservableCollection<ItemInformation>();

                SetItemInformationToItems(Items);
            });
        }

        private static void SetItemInformationToItems(ObservableCollection<Item> items)
        {
            if (items == null) return;

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