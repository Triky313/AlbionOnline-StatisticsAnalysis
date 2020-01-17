using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace StatisticsAnalysisTool
{
    public class StatisticsAnalysisManager
    {
        // Info Link -> https://github.com/broderickhyman/ao-bin-dumps
        // Models: https://github.com/broderickhyman/albiondata-models-dotNet

        public static List<Item> Items;
        public static int RefreshRate = Settings.Default.RefreshRate;
        public static int UpdateItemListByDays = Settings.Default.UpdateItemListByDays;
        public static string ItemListSourceUrl = Settings.Default.DefaultItemListSourceUrl;
        
        public static async Task<bool> GetItemsFromJsonAsync()
        {
            var url = ItemListSourceUrl;

            if (string.IsNullOrEmpty(ItemListSourceUrl))
            {
                url = Settings.Default.DefaultItemListSourceUrl;
                if (string.IsNullOrEmpty(url))
                    return false;

                var ini = new IniFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.SettingsFileName));
                ini.WriteValue("Settings", "ItemListSourceUrl", url);
                MessageBox.Show(LanguageController.Translation("DEFAULT_ITEMLIST_HAS_BEEN_LOADED"), LanguageController.Translation("NOTE"));
            }

            if (File.Exists($"{AppDomain.CurrentDomain.BaseDirectory}{Settings.Default.ItemListFileName}"))
            {
                var fileDateTime = File.GetLastWriteTime($"{AppDomain.CurrentDomain.BaseDirectory}{Settings.Default.ItemListFileName}");

                if (fileDateTime.AddDays(7) < DateTime.Now)
                {
                    using (var wc = new WebClient())
                    {
                        var itemString = await wc.DownloadStringTaskAsync(url);
                        File.WriteAllText($"{AppDomain.CurrentDomain.BaseDirectory}{Settings.Default.ItemListFileName}", itemString, Encoding.UTF8);
                        
                        try
                        {
                            Items = JsonConvert.DeserializeObject<List<Item>>(itemString);
                        }
                        catch (Exception ex)
                        {
                            Debug.Print(ex.Message);
                            Items = null;
                            return false;
                        }
                        return true;
                    }
                }

                try
                {
                    var localItemString = File.ReadAllText($"{AppDomain.CurrentDomain.BaseDirectory}{Settings.Default.ItemListFileName}");
                    Items = JsonConvert.DeserializeObject<List<Item>>(localItemString);
                }
                catch (Exception ex)
                {
                    Debug.Print(ex.Message);
                    Items = null;
                    return false;
                }

                return true;
            }

            using (var wd = new WebDownload(30000))
            {
                try
                {
                    var itemsString = await wd.DownloadStringTaskAsync(url);
                    File.WriteAllText($"{AppDomain.CurrentDomain.BaseDirectory}{Settings.Default.ItemListFileName}", itemsString, Encoding.UTF8);
                    Items = JsonConvert.DeserializeObject<List<Item>>(itemsString);
                    return true;
                }
                catch (Exception)
                {
                    try
                    {
                        var itemsString = await wd.DownloadStringTaskAsync(Settings.Default.DefaultItemListSourceUrl);
                        File.WriteAllText($"{AppDomain.CurrentDomain.BaseDirectory}{Settings.Default.ItemListFileName}", itemsString, Encoding.UTF8);
                        Items = JsonConvert.DeserializeObject<List<Item>>(itemsString);
                        return true;
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }
            }
        }

        public static async Task<ItemData> GetItemDataFromJsonAsync(Item item)
        {
            try
            {
                using (var wc = new WebClient())
                {
                    var itemDataJsonUrl =
                        $"https://gameinfo.albiononline.com/api/gameinfo/items/{item.UniqueName}/data";
                    var itemString = await wc.DownloadStringTaskAsync(itemDataJsonUrl);
                    var parsedObject = JObject.Parse(itemString);

                    var itemData = new ItemData
                    {
                        ItemType = (string) parsedObject["itemType"],
                        UniqueName = (string) parsedObject["uniqueName"],
                        //UiSprite = (string)parsedObject["uiSprite"],
                        Showinmarketplace = (bool) parsedObject["showinmarketplace"],
                        Level = (int) parsedObject["level"],
                        Tier = (int) parsedObject["tier"],
                        LocalizedNames = new List<ItemData.KeyValueStruct>(),
                        //CategoryId = (string)parsedObject["categoryId"],
                        //CategoryName = (string)parsedObject["categoryName"],
                        //LocalizedDescriptions = (string)parsedObject["localizedDescriptions"]["DE-DE"],
                        //SlotType = (string)parsedObject["slotType"],
                        //Stackable = (bool)parsedObject["stackable"],
                        //Equipable = (bool)parsedObject["equipable"],
                    };

                    AddLocalizedName(ref itemData, FrequentlyValues.GameLanguage.UnitedStates, parsedObject);
                    AddLocalizedName(ref itemData, FrequentlyValues.GameLanguage.Germany, parsedObject);
                    AddLocalizedName(ref itemData, FrequentlyValues.GameLanguage.Russia, parsedObject);
                    AddLocalizedName(ref itemData, FrequentlyValues.GameLanguage.Poland, parsedObject);
                    AddLocalizedName(ref itemData, FrequentlyValues.GameLanguage.Brazil, parsedObject);
                    AddLocalizedName(ref itemData, FrequentlyValues.GameLanguage.France, parsedObject);
                    AddLocalizedName(ref itemData, FrequentlyValues.GameLanguage.Spain, parsedObject);

                    return itemData;
                }
            }
            catch (Exception ex)
            {
                Debug.Print(ex.ToString());
                return null;
            }
        }

        public static async Task<List<Item>> FindItemsAsync(string searchText)
        {
            return await Task.Run(() =>
            {
                try
                {
                    return Items?.FindAll(s => (s.LocalizedName().ToLower().Contains(searchText.ToLower())));
                }
                catch (Exception ex)
                {
                    Debug.Print(ex.Message);
                    return null;
                }
            });
        }

        public static async Task<List<MarketResponse>> GetItemPricesFromJsonAsync(string uniqueName, bool showVillages = false)
        {
            using (var wc = new WebClient())
            {
                var statPricesDataJsonUrl = "https://www.albion-online-data.com/api/v2/stats/prices/" +
                                            uniqueName + 
                                            $"?locations={Locations.GetName(Location.Caerleon)}," +
                                            $"{Locations.GetName(Location.Bridgewatch)}," +
                                            $"{Locations.GetName(Location.Thetford)}," +
                                            $"{Locations.GetName(Location.FortSterling)}," +
                                            $"{Locations.GetName(Location.Lymhurst)}," +
                                            $"{Locations.GetName(Location.Martlock)},";

                if (showVillages)
                {
                    statPricesDataJsonUrl = "https://www.albion-online-data.com/api/v2/stats/prices/" +
                                            uniqueName +
                                            $"?locations={Locations.GetName(Location.Caerleon)}," +
                                            $"{Locations.GetName(Location.Bridgewatch)}," +
                                            $"{Locations.GetName(Location.Thetford)}," +
                                            $"{Locations.GetName(Location.FortSterling)}," +
                                            $"{Locations.GetName(Location.Lymhurst)}," +
                                            $"{Locations.GetName(Location.Martlock)}," +
                                            $"{Locations.GetName(Location.ForestCross)}," +
                                            $"{Locations.GetName(Location.SteppeCross)}," +
                                            $"{Locations.GetName(Location.HighlandCross)}," +
                                            $"{Locations.GetName(Location.MountainCross)}," +
                                            $"{Locations.GetName(Location.SwampCross)}," +
                                            $"{Locations.GetName(Location.BlackMarket)}";
                }

                var itemString = await wc.DownloadStringTaskAsync(statPricesDataJsonUrl);
                return JsonConvert.DeserializeObject<List<MarketResponse>>(itemString);
            }
        }
        
        public static FrequentlyValues.ItemTier GetItemTier(string uniqueName) => FrequentlyValues.ItemTiers.FirstOrDefault(x => x.Value == uniqueName.Split('_')[0]).Key;

        public static FrequentlyValues.ItemLevel GetItemLevel(string uniqueName)
        {
            if (!uniqueName.Contains("@"))
                return FrequentlyValues.ItemLevel.Level0;

            if(int.TryParse(uniqueName.Split('@')[1], out int number))
                return FrequentlyValues.ItemLevels.First(x => x.Value == number).Key;
            return FrequentlyValues.ItemLevel.Level0;
        }

        public static int GetQuality(FrequentlyValues.ItemQuality value) => FrequentlyValues.ItemQualities.FirstOrDefault(x => x.Key == value).Value;

        public static FrequentlyValues.ItemQuality GetQuality(int value) => FrequentlyValues.ItemQualities.FirstOrDefault(x => x.Value == value).Key;

        public static async Task<decimal> GetMarketStatAvgPriceAsync(string uniqueName, Location location)
        {
            using (var wc = new WebClient())
            {
                var apiString = 
                    $"https://www.albion-online-data.com/api/v1/stats/charts/" +
                    $"{uniqueName}?date={DateTime.Now:MM-dd-yyyy}?locations={Locations.GetName(location)}";

                try
                {
                    var itemString = await wc.DownloadStringTaskAsync(apiString);
                    var values = JsonConvert.DeserializeObject<List<MarketStatChartResponse>>(itemString);

                    return values.FirstOrDefault()?.Data.PricesAvg.FirstOrDefault() ?? 0;
                }
                catch (Exception ex)
                {
                    Debug.Print(ex.Message);
                    return 0;
                }
            }
        }

        private static void AddLocalizedName(ref ItemData itemData, FrequentlyValues.GameLanguage gameLanguage, JObject parsedObject)
        {
            var cultureCode = FrequentlyValues.GameLanguages.FirstOrDefault(x => x.Key == gameLanguage).Value;

            if (parsedObject["localizedNames"][cultureCode] != null)
                itemData.LocalizedNames.Add(new ItemData.KeyValueStruct() { Key = cultureCode, Value = parsedObject["localizedNames"][cultureCode].ToString() });
        }

    }
}