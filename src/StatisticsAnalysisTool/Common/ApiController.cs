using Serilog;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Exceptions;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Models.ApiModel;
using StatisticsAnalysisTool.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Common;

public static class ApiController
{
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

        var url = Path.Combine(GetAoDataProjectServerBaseUrlByCurrentServer(), "stats/prices/");
        url += uniqueName;

        if (marketLocations?.Count >= 1)
        {
            url += "?locations=";
            url = marketLocations.Aggregate(url, (current, location) => current + $"{(int) location},");
        }

        if (qualities?.Count >= 1)
        {
            url += "&qualities=";
            url = qualities.Aggregate(url, (current, quality) => current + $"{quality},");
        }

        using var clientHandler = new HttpClientHandler();
        clientHandler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
        using var client = new HttpClient(clientHandler);
        client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));

        try
        {
            client.Timeout = TimeSpan.FromSeconds(30);
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            using var response = await client.GetAsync(url);
            if (response.StatusCode == (HttpStatusCode) 429)
            {
                throw new TooManyRequestsException();
            }

            response.EnsureSuccessStatusCode();

            Stream decompressedStream = await DecompressedStream(response);

            var result = await JsonSerializer.DeserializeAsync<List<MarketResponse>>(decompressedStream);
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
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
            return null;
        }
    }

    public static async Task<List<MarketHistoriesResponse>> GetHistoryItemPricesFromJsonAsync(string uniqueName, IList<MarketLocation> locations, DateTime? date, int quality, int timeScale = 24)
    {
        var locationsString = "";

        if (locations?.Count > 0)
        {
            locationsString = string.Join(",", locations.Select(x => ((int) x).ToString()));
        }

        var qualitiesString = quality.ToString();

        var url = Path.Combine(GetAoDataProjectServerBaseUrlByCurrentServer(), "stats/history/");
        url += uniqueName;
        url += $"?locations={locationsString}";
        url += $"&date={date:M-d-yy}";
        url += $"&qualities={qualitiesString}";
        url += $"&time-scale={timeScale}";

        using var clientHandler = new HttpClientHandler();
        clientHandler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
        using var client = new HttpClient(clientHandler);
        client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
        client.Timeout = TimeSpan.FromSeconds(300);

        try
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            using var response = await client.GetAsync(url);

            if (response.StatusCode == (HttpStatusCode) 429)
            {
                throw new TooManyRequestsException();
            }

            response.EnsureSuccessStatusCode();

            await using var responseStream = await response.Content.ReadAsStreamAsync();
            using var memoryStream = new MemoryStream();
            await responseStream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;
            
            Stream decompressedStream = memoryStream;
            if (response.Content.Headers.ContentEncoding.Contains("gzip"))
            {
                decompressedStream = new GZipStream(memoryStream, CompressionMode.Decompress);
            }

            var result = await JsonSerializer.DeserializeAsync<List<MarketHistoriesResponse>>(decompressedStream);
            return MergeCityAndPortalCity(result);
        }
        catch (Exception e)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
            return null;
        }
    }

    public static async Task<GameInfoSearchResponse> GetGameInfoSearchFromJsonAsync(string username)
    {
        var gameInfoSearchResponse = new GameInfoSearchResponse();
        var url = $"{GetServerBaseUrlByCurrentServer()}/api/gameinfo/search?q={username}";

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
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
            return gameInfoSearchResponse;
        }
    }

    public static async Task<GameInfoPlayersResponse> GetGameInfoPlayersFromJsonAsync(string userid)
    {
        var gameInfoPlayerResponse = new GameInfoPlayersResponse();
        var url = $"{GetServerBaseUrlByCurrentServer()}/api/gameinfo/players/{userid}";

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
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
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
        var url = $"{GetServerBaseUrlByCurrentServer()}/api/gameinfo/players/{userid}/{killsDeathsExtensionString}";

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
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
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

        var url = $"{GetServerBaseUrlByCurrentServer()}/api/gameinfo/players/{userid}/topkills?range={unitOfTimeString}&offset=0";

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
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
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

        var url = $"{GetServerBaseUrlByCurrentServer()}/api/gameinfo/players/{userid}/solokills?range={unitOfTimeString}&offset=0";

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
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
            return values;
        }
    }

    //public static async Task<GameInfoGuildsResponse> GetGameInfoGuildsFromJsonAsync(string guildId)
    //{
    //    var url = $"{GetServerBaseUrlByCurrentServer()}/api/gameinfo/guilds/{guildId}";

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
    //            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
    //            return null;
    //        }
    //    }
    //}

    public static async Task<List<GoldResponseModel>> GetGoldPricesFromJsonAsync(int count, int timeout = 300)
    {
        var url = Path.Combine(GetAoDataProjectServerBaseUrlByCurrentServer(), "stats/");
        url += $"gold?count={count}";

        using var clientHandler = new HttpClientHandler();
        clientHandler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
        using var client = new HttpClient(clientHandler);
        client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
        client.Timeout = TimeSpan.FromSeconds(timeout);
        try
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            using var response = await client.GetAsync(url);

            response.EnsureSuccessStatusCode();

            await using var responseStream = await response.Content.ReadAsStreamAsync();
            Stream decompressedStream = responseStream;
            if (response.Content.Headers.ContentEncoding.Contains("gzip"))
            {
                decompressedStream = new GZipStream(responseStream, CompressionMode.Decompress);
            }

            var result = await JsonSerializer.DeserializeAsync<List<GoldResponseModel>>(decompressedStream);
            return result ?? new List<GoldResponseModel>();
        }
        catch (Exception e)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
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
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
            return values;
        }
    }

    //private static async Task<Stream> DecompressedStream(HttpResponseMessage response)
    //{
    //    await using var responseStream = await response.Content.ReadAsStreamAsync();
    //    Stream decompressedStream = responseStream;
    //    if (response.Content.Headers.ContentEncoding.Contains("gzip"))
    //    {
    //        decompressedStream = new GZipStream(responseStream, CompressionMode.Decompress);
    //    }

    //    return decompressedStream;
    //}

    private static async Task<Stream> DecompressedStream(HttpResponseMessage response)
    {
        var responseStream = await response.Content.ReadAsStreamAsync();
        var memoryStream = new MemoryStream();
        await responseStream.CopyToAsync(memoryStream);
        memoryStream.Position = 0;

        if (response.Content.Headers.ContentEncoding.Contains("gzip"))
        {
            return new GZipStream(memoryStream, CompressionMode.Decompress);
        }

        memoryStream.Position = 0;
        return memoryStream;
    }

    #region Helper methods

    #region Merge history data

    private static List<MarketHistoriesResponse> MergeCityAndPortalCity(List<MarketHistoriesResponse> values)
    {
        var fortSterlingMarketResponses = values.Where(x => x.Location.GetMarketLocationByLocationNameOrId() is MarketLocation.FortSterlingMarket or MarketLocation.FortSterlingPortal).ToList();
        SetMarketHistoriesResponseByQuality(fortSterlingMarketResponses, values, MarketLocation.FortSterlingMarket);

        var martlockMarketResponses = values.Where(x => x.Location.GetMarketLocationByLocationNameOrId() is MarketLocation.MartlockMarket or MarketLocation.MartlockPortal).ToList();
        SetMarketHistoriesResponseByQuality(martlockMarketResponses, values, MarketLocation.MartlockMarket);

        var lymhurstMarketResponses = values.Where(x => x.Location.GetMarketLocationByLocationNameOrId() is MarketLocation.LymhurstMarket or MarketLocation.LymhurstPortal).ToList();
        SetMarketHistoriesResponseByQuality(lymhurstMarketResponses, values, MarketLocation.LymhurstMarket);

        var thetfordMarketResponses = values.Where(x => x.Location.GetMarketLocationByLocationNameOrId() is MarketLocation.ThetfordMarket or MarketLocation.ThetfordPortal).ToList();
        SetMarketHistoriesResponseByQuality(thetfordMarketResponses, values, MarketLocation.ThetfordMarket);

        var bridgewatchMarketResponses = values.Where(x => x.Location.GetMarketLocationByLocationNameOrId() is MarketLocation.BridgewatchMarket or MarketLocation.BridgewatchPortal).ToList();
        SetMarketHistoriesResponseByQuality(bridgewatchMarketResponses, values, MarketLocation.BridgewatchMarket);

        return values;
    }

    private static void SetMarketHistoriesResponseByQuality(List<MarketHistoriesResponse> filteredValues, List<MarketHistoriesResponse> usedValues, MarketLocation marketLocation)
    {
        for (var i = 1; i <= 5; i++)
        {
            var marketResponsesByQuality = GetMarketHistoriesResponseByQuality(i, filteredValues);
            var result = MergeMarketAndPortalPrices(marketResponsesByQuality, Locations.GetDisplayName(marketLocation));

            if (string.IsNullOrEmpty(result?.ItemId))
            {
                continue;
            }

            foreach (var marketResponse in marketResponsesByQuality)
            {
                usedValues.Remove(marketResponse);
            }
            usedValues.Add(result);
        }
    }

    private static List<MarketHistoriesResponse> GetMarketHistoriesResponseByQuality(int quality, IEnumerable<MarketHistoriesResponse> marketResponses)
    {
        return marketResponses.Where(x => x.Quality == quality).ToList();
    }

    private static MarketHistoriesResponse MergeMarketAndPortalPrices(List<MarketHistoriesResponse> list, string locationName)
    {
        foreach (var marketResponse in list)
        {
            marketResponse.Location = locationName;
        }

        var marketHistoryResult = list.FirstOrDefault();
        foreach (var marketResponse in list)
        {
            if (marketResponse?.Data?.Max(x => x?.Timestamp) > marketHistoryResult?.Data?.Max(x => x?.Timestamp))
            {
                marketHistoryResult = marketResponse;
            }

        }

        return marketHistoryResult;
    }

    #endregion

    #region Merge market data

    private static List<MarketResponse> MergeMarketAndPortalLocations(List<MarketResponse> values)
    {
        var fortSterlingMarketResponses = values.Where(x => x.City.GetMarketLocationByLocationNameOrId() is MarketLocation.FortSterlingMarket or MarketLocation.FortSterlingPortal).ToList();
        SetMarketResponseByQuality(fortSterlingMarketResponses, values, MarketLocation.FortSterlingMarket);

        var martlockMarketResponses = values.Where(x => x.City.GetMarketLocationByLocationNameOrId() is MarketLocation.MartlockMarket or MarketLocation.MartlockPortal).ToList();
        SetMarketResponseByQuality(martlockMarketResponses, values, MarketLocation.MartlockMarket);

        var lymhurstMarketResponses = values.Where(x => x.City.GetMarketLocationByLocationNameOrId() is MarketLocation.LymhurstMarket or MarketLocation.LymhurstPortal).ToList();
        SetMarketResponseByQuality(lymhurstMarketResponses, values, MarketLocation.LymhurstMarket);

        var thetfordMarketResponses = values.Where(x => x.City.GetMarketLocationByLocationNameOrId() is MarketLocation.ThetfordMarket or MarketLocation.ThetfordPortal).ToList();
        SetMarketResponseByQuality(thetfordMarketResponses, values, MarketLocation.ThetfordMarket);

        var bridgewatchMarketResponses = values.Where(x => x.City.GetMarketLocationByLocationNameOrId() is MarketLocation.BridgewatchMarket or MarketLocation.BridgewatchPortal).ToList();
        SetMarketResponseByQuality(bridgewatchMarketResponses, values, MarketLocation.BridgewatchMarket);

        return values;
    }

    private static void SetMarketResponseByQuality(List<MarketResponse> filteredValues, List<MarketResponse> usedValues, MarketLocation marketLocation)
    {
        for (var i = 1; i <= 5; i++)
        {
            var marketResponsesByQuality = GetMarketResponsesByQuality(i, filteredValues);
            var result = MergeMarketAndPortalPrices(marketResponsesByQuality, Locations.GetDisplayName(marketLocation));

            if (string.IsNullOrEmpty(result.ItemTypeId))
            {
                continue;
            }

            foreach (var marketResponse in marketResponsesByQuality)
            {
                usedValues.Remove(marketResponse);
            }
            usedValues.Add(result);
        }
    }

    private static List<MarketResponse> GetMarketResponsesByQuality(int quality, IEnumerable<MarketResponse> marketResponses)
    {
        return marketResponses.Where(x => x.QualityLevel == quality).ToList();
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
            ItemTypeId = firstMarketResponse?.ItemTypeId ?? string.Empty,
            QualityLevel = (firstMarketResponse?.QualityLevel ?? -1)
        };
    }

    #endregion

    private static string GetAoDataProjectServerBaseUrlByCurrentServer()
    {
        return SettingsController.CurrentSettings.ServerLocation switch
        {
            ServerLocation.America => SettingsController.CurrentSettings.AlbionDataProjectBaseUrlWest,
            ServerLocation.Asia => SettingsController.CurrentSettings.AlbionDataProjectBaseUrlEast,
            ServerLocation.Europe => SettingsController.CurrentSettings.AlbionDataProjectBaseUrlEurope,
            _ => SettingsController.CurrentSettings.AlbionDataProjectBaseUrlWest
        };
    }

    private static string GetServerBaseUrlByCurrentServer()
    {
        return SettingsController.CurrentSettings.ServerLocation switch
        {
            ServerLocation.America => SettingsController.CurrentSettings.AlbionOnlineApiBaseUrlWest,
            ServerLocation.Asia => SettingsController.CurrentSettings.AlbionOnlineApiBaseUrlEast,
            ServerLocation.Europe => SettingsController.CurrentSettings.AlbionOnlineApiBaseUrlEurope,
            _ => SettingsController.CurrentSettings.AlbionOnlineApiBaseUrlWest
        };
    }

    #endregion
}