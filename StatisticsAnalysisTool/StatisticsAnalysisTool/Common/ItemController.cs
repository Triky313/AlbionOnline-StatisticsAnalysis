using Newtonsoft.Json;
using StatisticsAnalysisTool.Properties;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace StatisticsAnalysisTool.Common
{
    using Models;
    using System;
    using System.Linq;
    using System.Text;

    public class ItemController
    {
        public static ObservableCollection<Item> Items;

        private static readonly string FullItemInformationFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.FullItemInformationFileName);

        private static ObservableCollection<ItemInformation> _itemInformationList = new ObservableCollection<ItemInformation>();

        #region Item list

        public static async Task<bool> GetItemListFromJsonAsync()
        {
            var url = Settings.Default.ItemListSourceUrl;
            if (!GetItemListSourceUrlIfExist(ref url))
                return false;

            if (File.Exists($"{AppDomain.CurrentDomain.BaseDirectory}{Settings.Default.ItemListFileName}"))
            {
                var fileDateTime = File.GetLastWriteTime($"{AppDomain.CurrentDomain.BaseDirectory}{Settings.Default.ItemListFileName}");

                if (fileDateTime.AddDays(7) < DateTime.Now)
                {
                    Items = await TryToGetItemListFromWeb(url);
                    return (Items?.Count > 0);
                }

                Items = GetItemListFromLocal();
                return (Items?.Count > 0);
            }

            Items = await TryToGetItemListFromWeb(url);
            return (Items?.Count > 0);
        }

        private static bool GetItemListSourceUrlIfExist(ref string url)
        {
            if (string.IsNullOrEmpty(Settings.Default.ItemListSourceUrl))
            {
                url = Settings.Default.DefaultItemListSourceUrl;
                if (string.IsNullOrEmpty(url))
                    return false;

                Settings.Default.ItemListSourceUrl = Settings.Default.DefaultItemListSourceUrl;
                MessageBox.Show(LanguageController.Translation("DEFAULT_ITEMLIST_HAS_BEEN_LOADED"), LanguageController.Translation("NOTE"));
            }
            return true;
        }

        private static ObservableCollection<Item> GetItemListFromLocal()
        {
            try
            {
                var localItemString = File.ReadAllText($"{AppDomain.CurrentDomain.BaseDirectory}{Settings.Default.ItemListFileName}");
                return ConvertItemJsonObjectToItem(JsonConvert.DeserializeObject<ObservableCollection<ItemJsonObject>>(localItemString));
            }
            catch
            {
                return new ObservableCollection<Item>();
            }
        }

        private static ObservableCollection<Item> ConvertItemJsonObjectToItem(ObservableCollection<ItemJsonObject> itemJsonObjectList)
        {
            var result = itemJsonObjectList.Select(item => new Item()
            {
                LocalizationNameVariable = item.LocalizationNameVariable,
                LocalizationDescriptionVariable = item.LocalizationDescriptionVariable,
                LocalizedNames = item.LocalizedNames,
                Index = item.Index,
                UniqueName = item.UniqueName
            }).ToList();

            return new ObservableCollection<Item>(result);
        }

        private static async Task<ObservableCollection<Item>> TryToGetItemListFromWeb(string url)
        {
            using (var wd = new WebDownload(30000))
            {
                try
                {
                    var itemsString = await wd.DownloadStringTaskAsync(url);
                    File.WriteAllText($"{AppDomain.CurrentDomain.BaseDirectory}{Settings.Default.ItemListFileName}", itemsString, Encoding.UTF8);
                    return JsonConvert.DeserializeObject<ObservableCollection<Item>>(itemsString);
                }
                catch (Exception)
                {
                    try
                    {
                        var itemsString = await wd.DownloadStringTaskAsync(Settings.Default.DefaultItemListSourceUrl);
                        File.WriteAllText($"{AppDomain.CurrentDomain.BaseDirectory}{Settings.Default.ItemListFileName}", itemsString, Encoding.UTF8);
                        return JsonConvert.DeserializeObject<ObservableCollection<Item>>(itemsString);
                    }
                    catch
                    {
                        return null;
                    }
                }
            }
        }

        #endregion

        public static string LocalizedName(LocalizedNames localizedNames, string currentLanguage = null, string alternativeName = "NO_ITEM_NAME")
        {
            if (localizedNames == null)
                return alternativeName;

            if (string.IsNullOrEmpty(currentLanguage))
                currentLanguage = LanguageController.CurrentCultureInfo.TextInfo.CultureName.ToUpper();

            switch (FrequentlyValues.GameLanguages.FirstOrDefault(x => string.Equals(x.Value, currentLanguage, StringComparison.CurrentCultureIgnoreCase)).Key)
            {
                case GameLanguage.UnitedStates:
                    return Encoding.UTF8.GetString(Encoding.Default.GetBytes(localizedNames.EnUs));
                case GameLanguage.Germany:
                    return Encoding.UTF8.GetString(Encoding.Default.GetBytes(localizedNames.DeDe));
                case GameLanguage.Russia:
                    return Encoding.UTF8.GetString(Encoding.Default.GetBytes(localizedNames.RuRu));
                case GameLanguage.Poland:
                    return Encoding.UTF8.GetString(Encoding.Default.GetBytes(localizedNames.PlPl));
                case GameLanguage.Brazil:
                    return Encoding.UTF8.GetString(Encoding.Default.GetBytes(localizedNames.PtBr));
                case GameLanguage.France:
                    return Encoding.UTF8.GetString(Encoding.Default.GetBytes(localizedNames.FrFr));
                case GameLanguage.Spain:
                    return Encoding.UTF8.GetString(Encoding.Default.GetBytes(localizedNames.EsEs));
                case GameLanguage.Chinese:
                    return Encoding.UTF8.GetString(Encoding.Default.GetBytes(localizedNames.ZhCn));
                default:
                    return alternativeName;
            }
        }

        public static int GetItemLevel(string uniqueName)
        {
            if (!uniqueName.Contains("@"))
            {
                return 0;
            }

            return int.TryParse(uniqueName.Split('@')[1], out int number) ? number : 0;
        }
        
        public static int GetItemTier(Item item)
        {
            var itemNameTierText = item.UniqueName.Split('_')[0];
            if (itemNameTierText.Substring(0, 1) == "T" && int.TryParse(itemNameTierText.Substring(1, 1), out var result))
            {
                return result;
            }

            if (item.FullItemInformation?.Tier != null)
            {
                return item.FullItemInformation.Tier;
            }

            return -1;
        }

        public static ItemQuality GetQuality(int value) => FrequentlyValues.ItemQualities.FirstOrDefault(x => x.Value == value).Key;
        
        public static Style LocationStyle(Location location) {
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

        #region ItemInformation

        public static bool IsFullItemInformationComplete => Items?.All(item => item.FullItemInformation != null) ?? false;

        public static async Task<ItemInformation> GetFullItemInformationAsync(Item item)
        {
            var itemInformation = _itemInformationList.FirstOrDefault(x => x.UniqueName == item?.UniqueName);

            if (itemInformation?.HttpStatus == HttpStatusCode.NotFound)
            {
                return itemInformation;
            }

            if (string.IsNullOrEmpty(itemInformation?.UniqueName) || !IsItemInformationUpToDate(itemInformation.LastUpdate) || itemInformation.CategoryObject?.ParentCategory == null)
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

            var localItemInfo = _itemInformationList.FirstOrDefault(x => x.UniqueName == currentItemInformation.UniqueName);
            _itemInformationList.Remove(localItemInfo);

            currentItemInformation.LastUpdate = DateTime.Now;
            _itemInformationList.Add(currentItemInformation);
        }

        private static bool IsItemInformationUpToDate(DateTime? lastUpdate)
        {
            if (lastUpdate == null || lastUpdate.Value.Year == 1)
            {
                return false;
            }

            var lastUpdateWithCycleDays =  lastUpdate.Value.AddDays(Settings.Default.FullItemInformationUpdateCycleDays);
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
                if (_itemInformationList != null && _itemInformationList.Count > 0)
                {
                    return;
                }

                if (File.Exists(FullItemInformationFilePath))
                {
                    using (var streamReader = new StreamReader(FullItemInformationFilePath, Encoding.UTF8))
                    {
                        var readContents = streamReader.ReadToEnd();
                        _itemInformationList = JsonConvert.DeserializeObject<ObservableCollection<ItemInformation>>(readContents);
                    }
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

            foreach (var item in items)
            {
                var itemInformation = _itemInformationList.FirstOrDefault(x => x.UniqueName == item?.UniqueName);
                item.FullItemInformation = itemInformation;
            }
        }
        
        #endregion
    }
}