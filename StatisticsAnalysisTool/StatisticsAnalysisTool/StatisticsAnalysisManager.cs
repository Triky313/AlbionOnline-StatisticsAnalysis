using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Properties;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace StatisticsAnalysisTool
{
    public class StatisticsAnalysisManager
    {
        // Info Link -> https://github.com/broderickhyman/ao-bin-dumps
        // Models: https://github.com/broderickhyman/albiondata-models-dotNet

        private const string ItemsJsonUrl =
            "https://raw.githubusercontent.com/broderickhyman/ao-bin-dumps/master/formatted/items.json";
        private enum GameLanguage { UnitedStates, Germany, Russia, Poland, Brazil, France, Spain }
        public enum ItemTier { T1 = 0, T2 = 1, T3 = 2, T4 = 3, T5 = 4, T6 = 5, T7 = 6, T8 = 7 }
        public enum ItemLevel { Level0 = 0, Level1 = 1, Level2 = 2, Level3 = 3 }
        public enum ItemQuality { Normal = 0, Good = 1, Outstanding = 2, Excellent = 3, Masterpiece = 4 }

        private static readonly Dictionary<ItemTier, string> ItemTiers = new Dictionary<ItemTier, string>
        {
            {ItemTier.T1, "T1" },
            {ItemTier.T2, "T2" },
            {ItemTier.T3, "T3" },
            {ItemTier.T4, "T4" },
            {ItemTier.T5, "T5" },
            {ItemTier.T6, "T6" },
            {ItemTier.T7, "T7" },
            {ItemTier.T8, "T8" }
        };
        private static readonly Dictionary<ItemLevel, int> ItemLevels = new Dictionary<ItemLevel, int>
        {
            {ItemLevel.Level0, 0 },
            {ItemLevel.Level1, 1 },
            {ItemLevel.Level2, 2 },
            {ItemLevel.Level3, 3 }
        };
        private static readonly Dictionary<ItemQuality, int> ItemQualities = new Dictionary<ItemQuality, int>
        {
            {ItemQuality.Normal, 1 },
            {ItemQuality.Good, 2 },
            {ItemQuality.Outstanding, 3 },
            {ItemQuality.Excellent, 4 },
            {ItemQuality.Masterpiece, 5 }
        };
        private static readonly Dictionary<GameLanguage, string> GameLanguages = new Dictionary<GameLanguage, string>()
        {
            {GameLanguage.UnitedStates, "EN-US" },
            {GameLanguage.Germany, "DE-DE" },
            {GameLanguage.Russia, "RU-RU" },
            {GameLanguage.Poland, "PL-PL" },
            {GameLanguage.Brazil, "PT-BR" },
            {GameLanguage.France, "FR-FR" },
            {GameLanguage.Spain, "ES-ES" }
        };
        public static List<Item> Items;
        public static int RefreshRate = Settings.Default.RefreshRate;
        public static int UpdateItemListByDays = Settings.Default.UpdateItemListByDays;
        
        public static async Task<bool> GetItemsFromJsonAsync()
        {
            if (File.Exists($"{AppDomain.CurrentDomain.BaseDirectory}{Settings.Default.ItemListFileName}"))
            {
                var fileDateTime = File.GetLastWriteTime($"{AppDomain.CurrentDomain.BaseDirectory}{Settings.Default.ItemListFileName}");

                if (fileDateTime.AddDays(7) < DateTime.Now)
                {
                    using (var wc = new WebClient())
                    {
                        var itemString = await wc.DownloadStringTaskAsync(ItemsJsonUrl);
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

                var localItemString = File.ReadAllText($"{AppDomain.CurrentDomain.BaseDirectory}{Settings.Default.ItemListFileName}");

                try
                {
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

            using (var wc = new WebClient())
            {
                var itemsString = await wc.DownloadStringTaskAsync(ItemsJsonUrl);
                File.WriteAllText($"{AppDomain.CurrentDomain.BaseDirectory}{Settings.Default.ItemListFileName}", itemsString, Encoding.UTF8);
                Items = JsonConvert.DeserializeObject<List<Item>>(itemsString);
                return true;
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

                    AddLocalizedName(ref itemData, GameLanguage.UnitedStates, parsedObject);
                    AddLocalizedName(ref itemData, GameLanguage.Germany, parsedObject);
                    AddLocalizedName(ref itemData, GameLanguage.Russia, parsedObject);
                    AddLocalizedName(ref itemData, GameLanguage.Poland, parsedObject);
                    AddLocalizedName(ref itemData, GameLanguage.Brazil, parsedObject);
                    AddLocalizedName(ref itemData, GameLanguage.France, parsedObject);
                    AddLocalizedName(ref itemData, GameLanguage.Spain, parsedObject);

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
                                            "?locations=Caerleon,Bridgewatch,Thetford,FortSterling,Lymhurst,Martlock,";

                if (showVillages)
                    statPricesDataJsonUrl = "https://www.albion-online-data.com/api/v2/stats/prices/" +
                                            uniqueName +
                                            "?locations=Caerleon,Bridgewatch,Thetford,FortSterling,Lymhurst,Martlock," +
                                            "ForestCross,SteppeCross,HighlandCross,MountainCross,SwampCross,BlackMarket";

                var itemString = await wc.DownloadStringTaskAsync(statPricesDataJsonUrl);
                return JsonConvert.DeserializeObject<List<MarketResponse>>(itemString);
            }
        }
        
        public static ItemTier GetItemTier(string uniqueName) => ItemTiers.FirstOrDefault(x => x.Value == uniqueName.Split('_')[0]).Key;

        public static ItemLevel GetItemLevel(string uniqueName)
        {
            if (!uniqueName.Contains("@"))
                return ItemLevel.Level0;

            if(int.TryParse(uniqueName.Split('@')[1], out int number))
                return ItemLevels.First(x => x.Value == number).Key;
            return ItemLevel.Level0;
        }

        public static int GetQuality(ItemQuality value) => ItemQualities.FirstOrDefault(x => x.Key == value).Value;

        public static ItemQuality GetQuality(int value) => ItemQualities.FirstOrDefault(x => x.Value == value).Key;

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

        #region Support methods

        private static void AddLocalizedName(ref ItemData itemData, GameLanguage gameLanguage, JObject parsedObject)
        {
            var cultureCode = GameLanguages.FirstOrDefault(x => x.Key == gameLanguage).Value;

            if (parsedObject["localizedNames"][cultureCode] != null)
                itemData.LocalizedNames.Add(new ItemData.KeyValueStruct() { Key = cultureCode, Value = parsedObject["localizedNames"][cultureCode].ToString() });
        }

        #endregion
    }
}