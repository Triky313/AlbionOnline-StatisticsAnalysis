namespace StatisticsAnalysisTool.Common
{
    using Models;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;

    public class ApiController
    {

        public static async Task<ItemData> GetItemDataFromJsonAsync(Item item)
        {
            try
            {
                using (var wc = new WebClient())
                {
                    var itemDataJsonUrl = $"https://gameinfo.albiononline.com/api/gameinfo/items/{item.UniqueName}/data";
                    var itemString = await wc.DownloadStringTaskAsync(itemDataJsonUrl);
                    var parsedObject = JObject.Parse(itemString);

                    var itemData = new ItemData
                    {
                        ItemType = (string)parsedObject["itemType"],
                        UniqueName = (string)parsedObject["uniqueName"],
                        //UiSprite = (string)parsedObject["uiSprite"],
                        Showinmarketplace = (bool)parsedObject["showinmarketplace"],
                        Level = (int)parsedObject["level"],
                        Tier = (int)parsedObject["tier"],
                        LocalizedNames = new List<ItemData.KeyValueStruct>(),
                        //CategoryId = (string)parsedObject["categoryId"],
                        //CategoryName = (string)parsedObject["categoryName"],
                        //LocalizedDescriptions = (string)parsedObject["localizedDescriptions"]["DE-DE"],
                        //SlotType = (string)parsedObject["slotType"],
                        //Stackable = (bool)parsedObject["stackable"],
                        //Equipable = (bool)parsedObject["equipable"],
                    };

                    StatisticsAnalysisManager.AddLocalizedName(ref itemData, parsedObject);
                    return itemData;
                }
            }
            catch (Exception ex)
            {
                Debug.Print(ex.ToString());
                return null;
            }
        }

        public static async Task<List<MarketResponse>> GetCityItemPricesFromJsonAsync(string uniqueName, List<string> locations)
        {
            using (var wc = new WebClient())
            {
                var statPricesDataJsonUrl = "https://www.albion-online-data.com/api/v2/stats/prices/";
                statPricesDataJsonUrl += uniqueName;
                statPricesDataJsonUrl += $"?locations=";
                foreach (var location in locations)
                {
                    statPricesDataJsonUrl += $"{location},";
                }
                // TODO: Noch nicht fertig und eingebaut, ersetzt GetItemPricesFromJsonAsync!

                var itemString = await wc.DownloadStringTaskAsync(statPricesDataJsonUrl);
                return JsonConvert.DeserializeObject<List<MarketResponse>>(itemString);
            }
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

        public static async Task<decimal> GetMarketStatAvgPriceFromJsonAsync(string uniqueName, Location location)
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
                catch
                {
                    return 0;
                }
            }
        }

    }
}