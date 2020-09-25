using StatisticsAnalysisTool.Exceptions;
using System.Linq;
using System.Net;
using System.Net.Http;

namespace StatisticsAnalysisTool.Common
{
    using Models;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public static class ApiController
    {
        public static async Task<ItemInformation> GetItemInfoFromJsonAsync(string uniqueName)
        {
            var url = $"https://gameinfo.albiononline.com/api/gameinfo/items/{uniqueName}/data";

            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(30);
                try
                {
                    using (var response = await client.GetAsync(url))
                    {
                        using (var content = response.Content)
                        {
                            var emptyItemInfo = new ItemInformation()
                            {
                                UniqueName = uniqueName,
                                LastUpdate = DateTime.Now
                            };

                            if (response.StatusCode == HttpStatusCode.NotFound)
                            {
                                emptyItemInfo.HttpStatus = HttpStatusCode.NotFound;
                                return emptyItemInfo;
                            }

                            if (response.IsSuccessStatusCode)
                            {
                                var itemInfo = JsonConvert.DeserializeObject<ItemInformation>(await content.ReadAsStringAsync());
                                itemInfo.HttpStatus = HttpStatusCode.OK;
                                return itemInfo;
                            }

                            emptyItemInfo.HttpStatus = response.StatusCode;
                            return emptyItemInfo;
                        }
                    }
                }
                catch
                {
                    return null;
                }
            }
        }

        public static async Task<ItemInformation> GetItemInfoFromJsonAsync(Item item) => await GetItemInfoFromJsonAsync(item.UniqueName);

        /// <summary>
        /// Returns city item prices bye uniqueName, locations and qualities.
        /// </summary>
        /// <exception cref="TooManyRequestsException"></exception>
        public static async Task<List<MarketResponse>> GetCityItemPricesFromJsonAsync(string uniqueName, List<string> locations, List<int> qualities)
        {
            if (locations?.Count < 1)
            {
                return new List<MarketResponse>();
            }

            var url = "https://www.albion-online-data.com/api/v2/stats/prices/";
            url += uniqueName;
            url += "?locations=";
            url = (locations ?? new List<string>()).Aggregate(url, (current, location) => current + $"{location},");
            url += "&qualities=";

            if (qualities.Count >= 1)
            {
                url = qualities.Aggregate(url, (current, quality) => current + $"{quality},");
            }

            using (var client = new HttpClient())
            {
                try
                {
                    client.Timeout = TimeSpan.FromSeconds(30);

                    using (var response = await client.GetAsync(url))
                    {
                        if (response.StatusCode == (HttpStatusCode) 429)
                        {
                            throw new TooManyRequestsException();
                        }

                        using (var content = response.Content)
                        {
                            return JsonConvert.DeserializeObject<List<MarketResponse>>(await content.ReadAsStringAsync());
                        }
                    }
                }
                catch (TooManyRequestsException)
                {
                    throw new TooManyRequestsException();
                }
                catch
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

            var url = "https://www.albion-online-data.com/api/v2/stats/history/";
            url += uniqueName;
            url += $"?locations={locationsString}";
            url += $"&date={date:M-d-yy}";
            url += $"&qualities={qualitiesString}";
            url += $"&time-scale={timeScale}";

            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(30);
                try
                {
                    using (var response = await client.GetAsync(url))
                    {
                        using (var content = response.Content)
                        {
                            if (response.StatusCode == (HttpStatusCode)429)
                            {
                                throw new TooManyRequestsException();
                            }

                            return JsonConvert.DeserializeObject<List<MarketHistoriesResponse>>(await content.ReadAsStringAsync());
                        }
                    }
                }
                catch (TooManyRequestsException)
                {
                    throw new TooManyRequestsException();
                }
                catch
                {
                    return null;
                }
            }
        }

        public static async Task<GameInfoSearchResponse> GetGameInfoSearchFromJsonAsync(string username)
        {
            var gameInfoSearchResponse = new GameInfoSearchResponse();
            var url = $"https://gameinfo.albiononline.com/api/gameinfo/search?q={username}";

            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(30);
                try
                {
                    using (var response = await client.GetAsync(url))
                    {
                        using (var content = response.Content)
                        {
                            return JsonConvert.DeserializeObject<GameInfoSearchResponse>(await content.ReadAsStringAsync()) ?? gameInfoSearchResponse;
                        }
                    }
                }
                catch
                {
                    return gameInfoSearchResponse;
                }
            }
        }

        public static async Task<GameInfoPlayersResponse> GetGameInfoPlayersFromJsonAsync(string userid)
        {
            var gameInfoPlayerResponse = new GameInfoPlayersResponse();
            var url = $"https://gameinfo.albiononline.com/api/gameinfo/players/{userid}";

            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(30);
                try
                {
                    using (var response = await client.GetAsync(url))
                    {
                        using (var content = response.Content)
                        {
                            return JsonConvert.DeserializeObject<GameInfoPlayersResponse>(await content.ReadAsStringAsync()) ?? gameInfoPlayerResponse;
                        }
                    }
                }
                catch
                {
                    return gameInfoPlayerResponse;
                }
            }
        }

        public static async Task<GameInfoGuildsResponse> GetGameInfoGuildsFromJsonAsync(string guildId)
        {
            var url = $"https://gameinfo.albiononline.com/api/gameinfo/guilds/{guildId}";

            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(30);
                try
                {
                    using (var response = await client.GetAsync(url))
                    {
                        using (var content = response.Content)
                        {
                            return JsonConvert.DeserializeObject<GameInfoGuildsResponse>(await content.ReadAsStringAsync());
                        }
                    }
                }
                catch
                {
                    return null;
                }
            }
        }

        public static async Task<List<GoldResponseModel>> GetGoldPricesFromJsonAsync(DateTime? dateTime, int count)
        {
            var checkedDateTime = (dateTime != null) ? dateTime.ToString() : string.Empty;

            var url = $"https://www.albion-online-data.com/api/v2/stats/Gold?" +
                      $"date={checkedDateTime}" +
                      $"&count={count}";

            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(30);
                try
                {
                    using (var response = await client.GetAsync(url))
                    {
                        using (var content = response.Content)
                        {
                            return JsonConvert.DeserializeObject<List<GoldResponseModel>>(await content.ReadAsStringAsync());
                        }
                    }
                }
                catch
                {
                    return new List<GoldResponseModel>();
                }
            }
        }
    }
}