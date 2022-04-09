using log4net;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Exceptions;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Models.ApiModel;

namespace StatisticsAnalysisTool.Common
{
    public static class ApiController
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

        /// <summary>
        ///     Returns all city item prices bye uniqueName, locations and qualities.
        /// </summary>
        /// <exception cref="TooManyRequestsException"></exception>
        public static async Task<List<MarketResponse>> GetCityItemPricesFromJsonAsync(string uniqueName)
        {
            var locations = Locations.GetLocationsListByArea(true, true, true, true);
            return await GetCityItemPricesFromJsonAsync(uniqueName, locations, new List<int> { 1, 2, 3, 4, 5 });
        }

        /// <summary>
        ///     Returns city item prices bye uniqueName, locations and qualities.
        /// </summary>
        /// <exception cref="TooManyRequestsException"></exception>
        public static async Task<List<MarketResponse>> GetCityItemPricesFromJsonAsync(string uniqueName, List<string> locations, List<int> qualities)
        {
            var url = SettingsController.CurrentSettings.CityPricesApiUrl ?? Settings.Default.CityPricesApiUrlDefault;
            url += uniqueName;

            if (locations?.Count >= 1)
            {
                url += "?locations=";
                url = locations.Aggregate(url, (current, location) => current + $"{location},");
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

                using var response = await client.GetAsync(url);
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                if (response.StatusCode == (HttpStatusCode)429)
                {
                    throw new TooManyRequestsException();
                }

                using var content = response.Content;
                return JsonSerializer.Deserialize<List<MarketResponse>>(await content.ReadAsStringAsync());
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

        public static async Task<List<MarketHistoriesResponse>> GetHistoryItemPricesFromJsonAsync(string uniqueName, IList<string> locations,
            DateTime? date, IList<int> qualities, int timeScale = 24)
        {
            var locationsString = "";
            var qualitiesString = "";

            if (locations?.Count > 0)
                locationsString = string.Join(",", locations);

            if (qualities?.Count > 0)
                qualitiesString = string.Join(",", qualities);

            var url = SettingsController.CurrentSettings.CityPricesHistoryApiUrl ?? Settings.Default.CityPricesHistoryApiUrlDefault;
            url += uniqueName;
            url += $"?locations={locationsString}";
            url += $"&date={date:M-d-yy}";
            url += $"&qualities={qualitiesString}";
            url += $"&time-scale={timeScale}";

            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(60);
            try
            {
                using var response = await client.GetAsync(url);
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                using var content = response.Content;
                if (response.StatusCode == (HttpStatusCode)429)
                {
                    throw new TooManyRequestsException();
                }

                return JsonSerializer.Deserialize<List<MarketHistoriesResponse>>(await content.ReadAsStringAsync());
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
                using var response = await client.GetAsync(url);
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                using var content = response.Content;
                return JsonSerializer.Deserialize<GameInfoSearchResponse>(await content.ReadAsStringAsync()) ?? gameInfoSearchResponse;
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
                using var response = await client.GetAsync(url);
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
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

            var killsDeathsExtensionString = gameInfoPlayersType == GameInfoPlayersType.Kills ? "kills" : "deaths";
            var url = $"https://gameinfo.albiononline.com/api/gameinfo/players/{userid}/{killsDeathsExtensionString}";

            using var clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
            using var client = new HttpClient(clientHandler);
            client.Timeout = TimeSpan.FromSeconds(600);
            try
            {
                using var response = await client.GetAsync(url);
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
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
                using var response = await client.GetAsync(url);
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
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
                using var response = await client.GetAsync(url);
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
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

        public static async Task<List<GoldResponseModel>> GetGoldPricesFromJsonAsync(DateTime? dateTime, int count, int timeout = 30)
        {
            var checkedDateTime = dateTime != null ? dateTime.ToString() : string.Empty;

            var url = $"{SettingsController.CurrentSettings.GoldStatsApiUrl ?? Settings.Default.GoldStatsApiUrlDefault}?date={checkedDateTime}&count={count}";

            using var clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
            using var client = new HttpClient(clientHandler);
            client.Timeout = TimeSpan.FromSeconds(timeout);
            try
            {
                using var response = await client.GetAsync(url);
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
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
    }
}