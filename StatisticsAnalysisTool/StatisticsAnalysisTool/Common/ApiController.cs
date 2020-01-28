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
            if (locations?.Count < 1)
                return new List<MarketResponse>();

            using (var wc = new WebClient())
            {
                var statPricesDataJsonUrl = "https://www.albion-online-data.com/api/v2/stats/prices/";
                statPricesDataJsonUrl += uniqueName;
                statPricesDataJsonUrl += $"?locations=";
                foreach (var location in locations)
                {
                    statPricesDataJsonUrl += $"{location},";
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

        public static async Task<GameInfoSearchResponse> GetGameInfoSearchFromJsonAsync(string username)
        {
            using (var wc = new WebClient())
            {
                var apiString = $"https://gameinfo.albiononline.com/api/gameinfo/search?q={username}";
                var itemString = await wc.DownloadStringTaskAsync(apiString);
                
                return JsonConvert.DeserializeObject<GameInfoSearchResponse>(itemString);
            }
        }
        
        public static async Task<GameInfoPlayersResponse> GetGameInfoPlayersFromJsonAsync(string userid)
        {
            using (var wc = new WebClient())
            {
                var apiString = $"https://gameinfo.albiononline.com/api/gameinfo/players/{userid}";
                var itemString = await wc.DownloadStringTaskAsync(apiString);

                return JsonConvert.DeserializeObject<GameInfoPlayersResponse>(itemString);
            }
        }

        public static async Task<GameInfiGuildsResponse> GetGameInfoGuildsFromJsonAsync(string guildid)
        {
            using (var wc = new WebClient())
            {
                var apiString = $"https://gameinfo.albiononline.com/api/gameinfo/guilds/{guildid}";
                var itemString = await wc.DownloadStringTaskAsync(apiString);

                return JsonConvert.DeserializeObject<GameInfiGuildsResponse>(itemString);
            }
        }

    }
}