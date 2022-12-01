using log4net;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Exceptions;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Models.ApiModel;
using StatisticsAnalysisTool.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Common
{
    public static class ApiController
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

        /// <summary>
        ///     Returns a list of all city item prices by uniqueName.
        /// </summary>
        /// <exception cref="TooManyRequestsException"></exception>
        public static async Task<List<MarketResponse>> GetCityItemPricesFromJsonAsync(string uniqueName)
        {
            var locations = Locations.GetAllMarketLocations();
            return await GetCityItemPricesFromJsonAsync(uniqueName, locations, new List<int> { 1, 2, 3, 4, 5 });
        }

        /// <summary>
        ///     Returns a list of city item prices by uniqueName, locations and qualities.
        /// </summary>
        /// <exception cref="TooManyRequestsException"></exception>
        public static async Task<List<MarketResponse>> GetCityItemPricesFromJsonAsync(string uniqueName, List<MarketLocation> marketLocations, List<int> qualities)
        {
            if (string.IsNullOrEmpty(uniqueName))
            {
                return new List<MarketResponse>();
            }

            var url = SettingsController.CurrentSettings.CityPricesApiUrl ?? Settings.Default.CityPricesApiUrlDefault;
            url += uniqueName;

            if (marketLocations?.Count >= 1)
            {
                url += "?locations=";
                url = marketLocations.Aggregate(url, (current, location) => current + $"{(int)location},");
            }

            if (qualities?.Count >= 1)
            {
                url += "&qualities=";
                url = qualities.Aggregate(url, (current, quality) => current + $"{quality},");
            }

            using var clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
            using var client = new HttpClient(clientHandler);
            try
            {
                client.Timeout = TimeSpan.FromSeconds(30);

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                using var response = await client.GetAsync(url);
                if (response.StatusCode == (HttpStatusCode)429)
                {
                    throw new TooManyRequestsException();
                }

                using var content = response.Content;
                var result = JsonSerializer.Deserialize<List<MarketResponse>>(await content.ReadAsStringAsync());
                return MergeMarketAndPortalLocations(result);
            }
            catch (TooManyRequestsException)
            {
                ConsoleManager.WriteLineForWarning(MethodBase.GetCurrentMethod()?.DeclaringType, new TooManyRequestsException());
                throw new TooManyRequestsException();
            }
            catch (Exception e)
            {
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                return null;
            }
        }

        public static async Task<List<MarketHistoriesResponse>> GetHistoryItemPricesFromJsonAsync(string uniqueName, IList<MarketLocation> locations,
            DateTime? date, IList<int> qualities, int timeScale = 24)
        {
            var locationsString = "";
            var qualitiesString = "";

            if (locations?.Count > 0)
            {
                locationsString = string.Join(",", locations.Select(x => ((int)x).ToString()));
            }

            if (qualities?.Count > 0)
            {
                qualitiesString = string.Join(",", qualities);
            }

            var url = SettingsController.CurrentSettings.CityPricesHistoryApiUrl ?? Settings.Default.CityPricesHistoryApiUrlDefault;
            url += uniqueName;
            url += $"?locations={locationsString}";
            url += $"&date={date:M-d-yy}";
            url += $"&qualities={qualitiesString}";
            url += $"&time-scale={timeScale}";

            using var clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
            using var client = new HttpClient(clientHandler);
            client.Timeout = TimeSpan.FromSeconds(300);

            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                using var response = await client.GetAsync(url);
                using var content = response.Content;
                if (response.StatusCode == (HttpStatusCode)429)
                {
                    throw new TooManyRequestsException();
                }

                var result = JsonSerializer.Deserialize<List<MarketHistoriesResponse>>(await content.ReadAsStringAsync());
                return MergeCityAndPortalCity(result);
            }
            catch (Exception e)
            {
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                return null;
            }
        }

        public static async Task<GameInfoSearchResponse> GetGameInfoSearchFromJsonAsync(string username)
        {
            var gameInfoSearchResponse = new GameInfoSearchResponse();
            var url = $"https://gameinfo.albiononline.com/api/gameinfo/search?q={username}";

            using var clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
            using var client = new HttpClient(clientHandler);
            client.Timeout = TimeSpan.FromSeconds(600);

            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                using var response = await client.GetAsync(url);
                using var content = response.Content;
                return JsonSerializer.Deserialize<GameInfoSearchResponse>(await content.ReadAsStringAsync()) ?? gameInfoSearchResponse;
            }
            catch (JsonException ex)
            {
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, ex);
                return gameInfoSearchResponse;
            }
            catch (Exception e)
            {
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                return gameInfoSearchResponse;
            }
        }

        public static async Task<GameInfoPlayersResponse> GetGameInfoPlayersFromJsonAsync(string userid)
        {
            var gameInfoPlayerResponse = new GameInfoPlayersResponse();
            var url = $"https://gameinfo.albiononline.com/api/gameinfo/players/{userid}";

            using var clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
            using var client = new HttpClient(clientHandler);
            client.Timeout = TimeSpan.FromSeconds(120);
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                using var response = await client.GetAsync(url);
                using var content = response.Content;
                return JsonSerializer.Deserialize<GameInfoPlayersResponse>(await content.ReadAsStringAsync()) ??
                       gameInfoPlayerResponse;
            }
            catch (Exception e)
            {
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                return gameInfoPlayerResponse;
            }
        }

        public static async Task<List<GameInfoPlayerKillsDeaths>> GetGameInfoPlayerKillsDeathsFromJsonAsync(string userid, GameInfoPlayersType gameInfoPlayersType)
        {
            var values = new List<GameInfoPlayerKillsDeaths>();

            if (string.IsNullOrEmpty(userid))
            {
                return values;
            }

            var killsDeathsExtensionString = gameInfoPlayersType == GameInfoPlayersType.Kills ? "kills" : "deaths";
            var url = $"https://gameinfo.albiononline.com/api/gameinfo/players/{userid}/{killsDeathsExtensionString}";

            using var clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
            using var client = new HttpClient(clientHandler);
            client.Timeout = TimeSpan.FromSeconds(600);
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                using var response = await client.GetAsync(url);
                using var content = response.Content;
                return JsonSerializer.Deserialize<List<GameInfoPlayerKillsDeaths>>(await content.ReadAsStringAsync()) ?? values;
            }
            catch (Exception e)
            {
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                return values;
            }
        }

        public static async Task<List<GameInfoPlayerKillsDeaths>> GetGameInfoPlayerTopKillsFromJsonAsync(string userid, UnitOfTime unitOfTime)
        {
            var values = new List<GameInfoPlayerKillsDeaths>();

            if (string.IsNullOrEmpty(userid))
            {
                return values;
            }

            var unitOfTimeString = unitOfTime switch
            {
                UnitOfTime.Day => "day",
                UnitOfTime.Week => "week",
                UnitOfTime.LastWeek => "lastWeek",
                UnitOfTime.Month => "month",
                UnitOfTime.LastMonth => "lastMonth",
                _ => ""
            };

            var url = $"https://gameinfo.albiononline.com/api/gameinfo/players/{userid}/topkills?range={unitOfTimeString}&offset=0";

            using var clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
            using var client = new HttpClient(clientHandler);
            client.Timeout = TimeSpan.FromSeconds(600);
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                using var response = await client.GetAsync(url);
                using var content = response.Content;
                return JsonSerializer.Deserialize<List<GameInfoPlayerKillsDeaths>>(await content.ReadAsStringAsync()) ?? values;
            }
            catch (Exception e)
            {
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                return values;
            }
        }

        public static async Task<List<GameInfoPlayerKillsDeaths>> GetGameInfoPlayerSoloKillsFromJsonAsync(string userid, UnitOfTime unitOfTime)
        {
            var values = new List<GameInfoPlayerKillsDeaths>();

            if (string.IsNullOrEmpty(userid))
            {
                return values;
            }

            var unitOfTimeString = unitOfTime switch
            {
                UnitOfTime.Day => "day",
                UnitOfTime.Week => "week",
                UnitOfTime.LastWeek => "lastWeek",
                UnitOfTime.Month => "month",
                UnitOfTime.LastMonth => "lastMonth",
                _ => ""
            };

            var url = $"https://gameinfo.albiononline.com/api/gameinfo/players/{userid}/solokills?range={unitOfTimeString}&offset=0";

            using var clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
            using var client = new HttpClient(clientHandler);
            client.Timeout = TimeSpan.FromSeconds(600);
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                using var response = await client.GetAsync(url);
                using var content = response.Content;
                return JsonSerializer.Deserialize<List<GameInfoPlayerKillsDeaths>>(await content.ReadAsStringAsync()) ?? values;
            }
            catch (Exception e)
            {
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                return values;
            }
        }

        //public static async Task<GameInfoGuildsResponse> GetGameInfoGuildsFromJsonAsync(string guildId)
        //{
        //    var url = $"https://gameinfo.albiononline.com/api/gameinfo/guilds/{guildId}";

        //    using (var client = new HttpClient())
        //    {
        //        client.Timeout = TimeSpan.FromSeconds(30);
        //        try
        //        {
        //            using (var response = await client.GetAsync(url))
        //            {
        //                using (var content = response.Content)
        //                {
        //                    return JsonConvert.DeserializeObject<GameInfoGuildsResponse>(await content.ReadAsStringAsync());
        //                }
        //            }
        //        }
        //        catch (Exception e)
        //        {
        //            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
        //            Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, e);
        //            return null;
        //        }
        //    }
        //}

        public static async Task<List<GoldResponseModel>> GetGoldPricesFromJsonAsync(DateTime? dateTime, int count, int timeout = 300)
        {
            var checkedDateTime = dateTime != null ? dateTime.ToString() : string.Empty;

            var url = $"{SettingsController.CurrentSettings.GoldStatsApiUrl ?? Settings.Default.GoldStatsApiUrlDefault}?date={checkedDateTime}&count={count}";

            using var clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
            using var client = new HttpClient(clientHandler);
            client.Timeout = TimeSpan.FromSeconds(timeout);
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                using var response = await client.GetAsync(url);
                using var content = response.Content;
                var contentString = await content.ReadAsStringAsync();
                return string.IsNullOrEmpty(contentString) ? new List<GoldResponseModel>() : JsonSerializer.Deserialize<List<GoldResponseModel>>(contentString);
            }
            catch (Exception e)
            {
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                return new List<GoldResponseModel>();
            }
        }

        public static async Task<List<Donation>> GetDonationsFromJsonAsync()
        {
            var values = new List<Donation>();
            var url = Settings.Default.DonationsUrl;

            using var clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
            using var client = new HttpClient(clientHandler);
            client.Timeout = TimeSpan.FromSeconds(600);
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                using var response = await client.GetAsync(url);
                using var content = response.Content;
                return JsonSerializer.Deserialize<List<Donation>>(await content.ReadAsStringAsync()) ?? values;
            }
            catch (Exception e)
            {
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                return values;
            }
        }

        #region Helper methods

        // TODO: Rework and take the newest entry
        private static List<MarketHistoriesResponse> MergeCityAndPortalCity(List<MarketHistoriesResponse> values)
        {
            foreach (var marketHistoriesResponse in values.Where(x => Locations.GetMarketLocationByLocationNameOrId(x.Location) is MarketLocation.FortSterlingMarket or MarketLocation.FortSterlingPortal))
            {
                marketHistoriesResponse.Location = "FortSterling";
            }

            foreach (var marketHistoriesResponse in values.Where(x => Locations.GetMarketLocationByLocationNameOrId(x.Location) is MarketLocation.MartlockMarket or MarketLocation.MartlockPortal))
            {
                marketHistoriesResponse.Location = "Martlock";
            }

            foreach (var marketHistoriesResponse in values.Where(x => Locations.GetMarketLocationByLocationNameOrId(x.Location) is MarketLocation.LymhurstMarket or MarketLocation.LymhurstPortal))
            {
                marketHistoriesResponse.Location = "Lymhurst";
            }

            foreach (var marketHistoriesResponse in values.Where(x => Locations.GetMarketLocationByLocationNameOrId(x.Location) is MarketLocation.ThetfordMarket or MarketLocation.ThetfordPortal))
            {
                marketHistoriesResponse.Location = "Thetford";
            }

            foreach (var marketHistoriesResponse in values.Where(x => Locations.GetMarketLocationByLocationNameOrId(x.Location) is MarketLocation.BridgewatchMarket or MarketLocation.BridgewatchPortal))
            {
                marketHistoriesResponse.Location = "Bridgewatch";
            }

            return values;
        }
        
        private static List<MarketResponse> MergeMarketAndPortalLocations(List<MarketResponse> values)
        {
            var fortSterlingMarketResponses = values.Where(x => x.City.GetMarketLocationByLocationNameOrId() is MarketLocation.FortSterlingMarket or MarketLocation.FortSterlingPortal).ToList();
            var fortSterlingResult = MergeMarketAndPortalPrices(fortSterlingMarketResponses, Locations.GetDisplayName(MarketLocation.FortSterlingMarket));
            foreach (var marketResponse in fortSterlingMarketResponses)
            {
                values.Remove(marketResponse);
            }
            values.Add(fortSterlingResult);

            var martlockMarketResponses = values.Where(x => x.City.GetMarketLocationByLocationNameOrId() is MarketLocation.MartlockMarket or MarketLocation.MartlockPortal).ToList();
            var martlockResult = MergeMarketAndPortalPrices(martlockMarketResponses, Locations.GetDisplayName(MarketLocation.MartlockMarket));
            foreach (var marketResponse in martlockMarketResponses)
            {
                values.Remove(marketResponse);
            }
            values.Add(martlockResult);

            var lymhurstMarketResponses = values.Where(x => x.City.GetMarketLocationByLocationNameOrId() is MarketLocation.LymhurstMarket or MarketLocation.LymhurstPortal).ToList();
            var lymhurstResult = MergeMarketAndPortalPrices(lymhurstMarketResponses, Locations.GetDisplayName(MarketLocation.LymhurstMarket));
            foreach (var marketResponse in lymhurstMarketResponses)
            {
                values.Remove(marketResponse);
            }
            values.Add(lymhurstResult);

            var thetfordMarketResponses = values.Where(x => x.City.GetMarketLocationByLocationNameOrId() is MarketLocation.ThetfordMarket or MarketLocation.ThetfordPortal).ToList();
            var thetfordResult = MergeMarketAndPortalPrices(thetfordMarketResponses, Locations.GetDisplayName(MarketLocation.ThetfordMarket));
            foreach (var marketResponse in thetfordMarketResponses)
            {
                values.Remove(marketResponse);
            }
            values.Add(thetfordResult);

            var bridgewatchMarketResponses = values.Where(x => x.City.GetMarketLocationByLocationNameOrId() is MarketLocation.BridgewatchMarket or MarketLocation.BridgewatchPortal).ToList();
            var bridgewatchResult = MergeMarketAndPortalPrices(bridgewatchMarketResponses, Locations.GetDisplayName(MarketLocation.BridgewatchMarket));
            foreach (var marketResponse in bridgewatchMarketResponses)
            {
                values.Remove(marketResponse);
            }
            values.Add(bridgewatchResult);
            
            return values;
        }
        
        private static MarketResponse MergeMarketAndPortalPrices(List<MarketResponse> list, string locationName)
        {
            foreach (var marketResponse in list)
            {
                marketResponse.City = locationName;
            }

            var buyPriceMaxDate = DateTime.MinValue;
            var buyPriceMax = 0UL;
            var buyPriceMinDate = DateTime.MinValue;
            var buyPriceMin = 0UL;
            var sellPriceMaxDate = DateTime.MinValue;
            var sellPriceMax = 0UL;
            var sellPriceMinDate = DateTime.MinValue;
            var sellPriceMin = 0UL;

            foreach (var marketResponse in list)
            {
                if (marketResponse.BuyPriceMaxDate > buyPriceMaxDate)
                {
                    buyPriceMaxDate = marketResponse.BuyPriceMaxDate;
                    buyPriceMax = marketResponse.BuyPriceMax;
                }

                if (marketResponse.BuyPriceMinDate > buyPriceMinDate)
                {
                    buyPriceMinDate = marketResponse.BuyPriceMinDate;
                    buyPriceMin = marketResponse.BuyPriceMin;
                }

                if (marketResponse.SellPriceMaxDate > sellPriceMaxDate)
                {
                    sellPriceMaxDate = marketResponse.SellPriceMaxDate;
                    sellPriceMax = marketResponse.SellPriceMax;
                }

                if (marketResponse.SellPriceMinDate > sellPriceMinDate)
                {
                    sellPriceMinDate = marketResponse.SellPriceMinDate;
                    sellPriceMin = marketResponse.SellPriceMin;
                }
            }

            var firstMarketResponse = list.FirstOrDefault();

            return new MarketResponse()
            {
                BuyPriceMax = buyPriceMax,
                BuyPriceMaxDate = buyPriceMaxDate,
                BuyPriceMin = buyPriceMin,
                BuyPriceMinDate = buyPriceMinDate,
                SellPriceMax = sellPriceMax,
                SellPriceMaxDate = sellPriceMaxDate,
                SellPriceMin = sellPriceMin,
                SellPriceMinDate = sellPriceMinDate,
                City = firstMarketResponse?.City,
                ItemTypeId = firstMarketResponse?.ItemTypeId
            };
        }

        #endregion
    }
}