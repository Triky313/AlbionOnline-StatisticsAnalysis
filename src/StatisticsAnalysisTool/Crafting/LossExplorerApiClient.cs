using Serilog;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Crafting;

public sealed class LossExplorerApiClient
{
    private const int MaximumAttempts = 3;
    private static readonly HttpClient HttpClient = CreateHttpClient();
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<IReadOnlyList<LossExplorerEvent>> GetEventsPageAsync(
        ServerLocation serverLocation,
        int limit,
        int offset,
        CancellationToken cancellationToken)
    {
        var baseUrl = GetGameInfoBaseUrl(serverLocation);
        var url = $"{baseUrl.TrimEnd('/')}/api/gameinfo/events?limit={limit}&offset={offset}&sort=recent";
        var response = await GetAsync<List<LossEventResponse>>(url, cancellationToken).ConfigureAwait(false);

        if (response == null)
        {
            return null;
        }

        return response
            .Where(x => x != null)
            .Select(MapEvent)
            .ToList();
    }

    public async Task<IReadOnlyList<MarketResponse>> GetPricesAsync(
        ServerLocation serverLocation,
        IReadOnlyCollection<string> itemUniqueNames,
        CancellationToken cancellationToken)
    {
        if (itemUniqueNames == null || itemUniqueNames.Count == 0)
        {
            return [];
        }

        var itemList = string.Join(",", itemUniqueNames.Select(Uri.EscapeDataString));
        var baseUrl = GetAlbionDataProjectBaseUrl(serverLocation);
        var url = $"{baseUrl.TrimEnd('/')}/stats/prices/{itemList}.json?qualities=1,2,3,4,5";
        return await GetAsync<List<MarketResponse>>(url, cancellationToken).ConfigureAwait(false);
    }

    private static LossExplorerEvent MapEvent(LossEventResponse response)
    {
        var equipmentItems = response.Victim?.Equipment?
            .Values
            .Where(IsValidItem)
            .Select(MapItem)
            .ToList() ?? [];

        var inventoryItems = response.Victim?.Inventory?
            .Where(IsValidItem)
            .Select(MapItem)
            .ToList() ?? [];

        return new LossExplorerEvent
        {
            EventId = response.EventId,
            TimeStampUtc = response.TimeStamp.Kind switch
            {
                DateTimeKind.Utc => response.TimeStamp,
                DateTimeKind.Local => response.TimeStamp.ToUniversalTime(),
                _ => DateTime.SpecifyKind(response.TimeStamp, DateTimeKind.Utc)
            },
            EquipmentItems = equipmentItems,
            InventoryItems = inventoryItems
        };
    }

    private static bool IsValidItem(LossItemResponse item)
    {
        return item != null && !string.IsNullOrWhiteSpace(item.Type) && item.Count > 0;
    }

    private static LossExplorerEventItem MapItem(LossItemResponse item)
    {
        return new LossExplorerEventItem
        {
            ItemUniqueName = item.Type,
            Count = item.Count,
            QualityLevel = NormalizeQualityLevel(item.Quality)
        };
    }

    private static int NormalizeQualityLevel(int qualityLevel)
    {
        return qualityLevel is >= 1 and <= 5 ? qualityLevel : 1;
    }

    private static async Task<T> GetAsync<T>(string url, CancellationToken cancellationToken) where T : class
    {
        for (var attempt = 1; attempt <= MaximumAttempts; attempt++)
        {
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(120));

            try
            {
                using var response = await HttpClient.GetAsync(
                    url,
                    HttpCompletionOption.ResponseHeadersRead,
                    timeoutCts.Token).ConfigureAwait(false);

                if (response.StatusCode == HttpStatusCode.TooManyRequests && attempt < MaximumAttempts)
                {
                    await DelayBeforeRetryAsync(attempt, cancellationToken).ConfigureAwait(false);
                    continue;
                }

                if (!response.IsSuccessStatusCode)
                {
                    Log.Warning(
                        "Loss Explorer request failed. Status={StatusCode}, Attempt={Attempt}, Url={Url}",
                        response.StatusCode,
                        attempt,
                        url);

                    if (attempt < MaximumAttempts && (int) response.StatusCode >= 500)
                    {
                        await DelayBeforeRetryAsync(attempt, cancellationToken).ConfigureAwait(false);
                        continue;
                    }

                    return null;
                }

                await using var responseStream = await response.Content.ReadAsStreamAsync(timeoutCts.Token).ConfigureAwait(false);
                return await JsonSerializer.DeserializeAsync<T>(responseStream, JsonOptions, timeoutCts.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested && attempt < MaximumAttempts)
            {
                Log.Warning("Loss Explorer request timed out. Attempt={Attempt}, Url={Url}", attempt, url);
                await DelayBeforeRetryAsync(attempt, cancellationToken).ConfigureAwait(false);
            }
            catch (HttpRequestException exception) when (attempt < MaximumAttempts)
            {
                Log.Warning(exception, "Loss Explorer request could not be completed. Attempt={Attempt}, Url={Url}", attempt, url);
                await DelayBeforeRetryAsync(attempt, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Loss Explorer request failed. Url={Url}", url);
                return null;
            }
        }

        return null;
    }

    private static Task DelayBeforeRetryAsync(int attempt, CancellationToken cancellationToken)
    {
        return Task.Delay(TimeSpan.FromSeconds(attempt * 2), cancellationToken);
    }

    private static string GetGameInfoBaseUrl(ServerLocation serverLocation)
    {
        var settings = SettingsController.CurrentSettings;
        return serverLocation switch
        {
            ServerLocation.America => settings.AlbionOnlineApiBaseUrlWest,
            ServerLocation.Asia => settings.AlbionOnlineApiBaseUrlEast,
            ServerLocation.Europe => settings.AlbionOnlineApiBaseUrlEurope,
            _ => settings.AlbionOnlineApiBaseUrlEurope
        };
    }

    private static string GetAlbionDataProjectBaseUrl(ServerLocation serverLocation)
    {
        var settings = SettingsController.CurrentSettings;
        return serverLocation switch
        {
            ServerLocation.America => settings.AlbionDataProjectBaseUrlWest,
            ServerLocation.Asia => settings.AlbionDataProjectBaseUrlEast,
            ServerLocation.Europe => settings.AlbionDataProjectBaseUrlEurope,
            _ => settings.AlbionDataProjectBaseUrlEurope
        };
    }

    private static HttpClient CreateHttpClient()
    {
        var handler = new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli
        };

        return new HttpClient(handler)
        {
            Timeout = Timeout.InfiniteTimeSpan
        };
    }

    private sealed class LossEventResponse
    {
        public long EventId { get; set; }

        public DateTime TimeStamp { get; set; }

        public LossVictimResponse Victim { get; set; }
    }

    private sealed class LossVictimResponse
    {
        public Dictionary<string, LossItemResponse> Equipment { get; set; } = [];

        public List<LossItemResponse> Inventory { get; set; } = [];
    }

    private sealed class LossItemResponse
    {
        public string Type { get; set; } = string.Empty;

        public int Count { get; set; }

        public int Quality { get; set; }
    }
}