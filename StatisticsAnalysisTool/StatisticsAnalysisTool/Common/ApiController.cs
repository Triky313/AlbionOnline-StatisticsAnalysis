using System.Diagnostics;

namespace StatisticsAnalysisTool.Common
{
    using Models;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading.Tasks;

    public static class ApiController
    {
        public static async Task<ItemInformation> GetItemInfoFromJsonAsync(string uniqueName)
        {
            using (var wc = new WebClient())
            {
                try
                {
                    var apiString = $"https://gameinfo.albiononline.com/api/gameinfo/items/{uniqueName}/data";
                    var itemString = await wc.DownloadStringTaskAsync(apiString).ConfigureAwait(true);

                    if (itemString == null)
                    {
                        return null;
                    }

                    var result = JsonConvert.DeserializeObject<ItemInformation>(itemString);
                    return result;
                }
                catch(Exception e)
                {
                    Debug.Print(e.Message);
                    Debug.Print(e.StackTrace);
                    return null;
                }
            }
        }

        public static async Task<ItemInformation> GetItemInfoFromJsonAsync(Item item)
        {
            return await GetItemInfoFromJsonAsync(item.UniqueName);
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
                foreach (var location in locations ?? new List<string>())
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

                try
                {
                    var itemString = await wc.DownloadStringTaskAsync(statPricesDataJsonUrl);
                    return JsonConvert.DeserializeObject<List<MarketResponse>>(itemString);
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        public static async Task<List<MarketHistoriesResponse>> GetHistoryItemPricesFromJsonAsync(string uniqueName, IList<string> locations, DateTime? date, IList<int> qualities, int timeScale = 24)
        {
            var locationsString = "";
            var qualitiesString = "";

            if (locations?.Count > 0)
                locationsString = string.Join(",", locations);

            if (qualities?.Count > 0)
                qualitiesString = string.Join(",", qualities);
            
            using (var wc = new WebClient())
            {
                var statPricesDataJsonUrl = "https://www.albion-online-data.com/api/v2/stats/history/";
                statPricesDataJsonUrl += uniqueName;
                statPricesDataJsonUrl += $"?locations={locationsString}";
                statPricesDataJsonUrl += $"&date={date:M-d-yy}";
                statPricesDataJsonUrl += $"&qualities={qualitiesString}";
                statPricesDataJsonUrl += $"&time-scale={timeScale}";

                var itemString = await wc.DownloadStringTaskAsync(statPricesDataJsonUrl);
                return JsonConvert.DeserializeObject<List<MarketHistoriesResponse>>(itemString);
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

        public static async Task<GameInfoGuildsResponse> GetGameInfoGuildsFromJsonAsync(string guildid)
        {
            using (var wc = new WebClient())
            {
                var apiString = $"https://gameinfo.albiononline.com/api/gameinfo/guilds/{guildid}";
                var itemString = await wc.DownloadStringTaskAsync(apiString);

                return JsonConvert.DeserializeObject<GameInfoGuildsResponse>(itemString);
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

        public static async Task<ItemCategoryTree> GetItemCategoryTreeAsync()
        {
            using (var wc = new WebClient())
            {
                try
                {
                    var apiString = "https://gameinfo.albiononline.com/api/gameinfo/items/_itemCategoryTree";
                    var itemString = await wc.DownloadStringTaskAsync(apiString);
                    var result = JsonConvert.DeserializeObject<ItemCategoryTree>(itemString);
                    return result;
                }
                catch
                {
                    return new ItemCategoryTree();
                }
            }
        }

        public static async Task<WeaponCategories> GetWeaponCategoriesAsync()
        {
            using (var wc = new WebClient())
            {
                try
                {
                    var apiString = "https://gameinfo.albiononline.com/api/gameinfo/items/_weaponCategories";
                    var itemString = await wc.DownloadStringTaskAsync(apiString);
                    var result = JsonConvert.DeserializeObject<WeaponCategories>(itemString);
                    return result;
                }
                catch
                {
                    return new WeaponCategories();
                }
            }
        }
    }
}