namespace StatisticsAnalysisTool.Common
{
    using Models;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;

    public class ApiController
    {

        public static async Task<ItemInformation> GetItemInfoFromJsonAsync(Item item)
        {
            using (var wc = new WebClient())
            {
                try
                {
                    var apiString = $"https://gameinfo.albiononline.com/api/gameinfo/items/{item.UniqueName}/data";
                    var itemString = await wc.DownloadStringTaskAsync(apiString);
                    var result = JsonConvert.DeserializeObject<ItemInformation>(itemString);
                    return result;
                }
                catch (ArgumentNullException)
                {
                    return null;
                }
                catch (WebException)
                {
                    return null;
                }
            }
        }

        public static async Task<List<MarketResponse>> GetCityItemPricesFromJsonAsync(string uniqueName, List<string> locations, List<int> qualities)
        {
            if (locations?.Count < 1)
                return new List<MarketResponse>();

            using (var wc = new WebClient())
            {
                var statPricesDataJsonUrl = "https://www.albion-online-data.com/api/v2/stats/prices/";
                statPricesDataJsonUrl += uniqueName;
                statPricesDataJsonUrl += "?locations=";
                foreach (var location in locations)
                {
                    statPricesDataJsonUrl += $"{location},";
                }
                statPricesDataJsonUrl += "&qualities=";
                if (qualities.Count >= 1)
                {
                    foreach (var quality in qualities)
                    {
                        statPricesDataJsonUrl += $"{quality},";
                    }
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
                    $"?locations={Locations.GetName(location)}{uniqueName}?date={DateTime.Now:MM-dd-yyyy}";

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
            var gameInfoSearchResponse = new GameInfoSearchResponse();

            using (var wc = new WebClient())
            {
                var apiString = $"https://gameinfo.albiononline.com/api/gameinfo/search?q={username}";
                var itemString = await wc.DownloadStringTaskAsync(apiString);
            
                var result = JsonConvert.DeserializeObject<GameInfoSearchResponse>(itemString);
                gameInfoSearchResponse = result ?? gameInfoSearchResponse;
                
                return gameInfoSearchResponse;
            }
        }
        
        public static async Task<GameInfoPlayersResponse> GetGameInfoPlayersFromJsonAsync(string userid)
        {
            var gameInfoPlayerResponse = new GameInfoPlayersResponse();

            using (var wc = new WebClient())
            {
                var apiString = $"https://gameinfo.albiononline.com/api/gameinfo/players/{userid}";
                var itemString = await wc.DownloadStringTaskAsync(apiString);

                var result = JsonConvert.DeserializeObject<GameInfoPlayersResponse>(itemString);
                gameInfoPlayerResponse = result ?? gameInfoPlayerResponse;
                return gameInfoPlayerResponse;
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

        public static async Task<List<GoldResponseModel>> GetGoldPricesFromJsonAsync(DateTime? dateTime, int count)
        {
            using (var wc = new WebClient())
            {
                var checkedDateTime = (dateTime != null)? dateTime.ToString() : "";
                var apiString =
                    "https://www.albion-online-data.com/api/v2/stats/Gold?" +
                    $"date={checkedDateTime}" +
                    $"&count={count}";

                try
                {
                    var itemString = await wc.DownloadStringTaskAsync(apiString);
                    return JsonConvert.DeserializeObject<List<GoldResponseModel>>(itemString);
                }
                catch
                {
                    return new List<GoldResponseModel>();
                }
            }
        }
    }
}