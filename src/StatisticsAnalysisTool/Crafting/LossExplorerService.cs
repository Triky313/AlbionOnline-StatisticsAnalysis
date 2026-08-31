using Serilog;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Crafting;

public sealed class LossExplorerService
{
    public const string CacheFileName = "LossExplorer.json";

    private const int PriceRequestMaximumLength = 3000;
    private static readonly TimeSpan PriceRefreshInterval = TimeSpan.FromDays(1);
    private readonly LossExplorerApiClient _apiClient = new();
    private readonly LossExplorerEventLoader _eventLoader = new();

    public async Task<LossExplorerCache> LoadCacheAsync(string cacheFilePath)
    {
        var cache = await FileController.LoadAsync<LossExplorerCache>(cacheFilePath).ConfigureAwait(false);
        cache = LossExplorerHistoryController.PrepareCache(cache, DateTime.UtcNow);
        await SaveCacheAsync(cache, cacheFilePath).ConfigureAwait(false);
        return cache;
    }

    public async Task<LossExplorerCache> UpdateAsync(
        LossExplorerCache cache,
        string cacheFilePath,
        ServerLocation serverLocation,
        Action<int> eventPageLoaded,
        Action<int, int> priceBatchLoaded,
        CancellationToken cancellationToken)
    {
        cache = LossExplorerHistoryController.PrepareCache(cache, DateTime.UtcNow);
        await LoadEventsAsync(
            cache,
            cacheFilePath,
            serverLocation,
            eventPageLoaded,
            cancellationToken).ConfigureAwait(false);

        await LoadMissingPricesAsync(
            cache,
            cacheFilePath,
            serverLocation,
            priceBatchLoaded,
            cancellationToken).ConfigureAwait(false);

        return cache;
    }

    private async Task LoadEventsAsync(
        LossExplorerCache cache,
        string cacheFilePath,
        ServerLocation serverLocation,
        Action<int> eventPageLoaded,
        CancellationToken cancellationToken)
    {
        var events = await _eventLoader.LoadNewEventsAsync(
            cache,
            serverLocation,
            eventPageLoaded,
            cancellationToken).ConfigureAwait(false);
        var syncUtc = DateTime.UtcNow;
        LossExplorerHistoryController.ApplyEvents(cache, events, syncUtc);
        cache.LastSuccessfulSyncUtc = syncUtc;
        await SaveCacheAsync(cache, cacheFilePath).ConfigureAwait(false);
    }

    private async Task LoadMissingPricesAsync(
        LossExplorerCache cache,
        string cacheFilePath,
        ServerLocation serverLocation,
        Action<int, int> priceBatchLoaded,
        CancellationToken cancellationToken)
    {
        var utcNow = DateTime.UtcNow;
        var refreshAllPrices = cache.LastPriceSyncUtc < utcNow.Subtract(PriceRefreshInterval);
        if (refreshAllPrices)
        {
            cache.Items.ForEach(x => x.HasPrice = false);
        }

        var missingItemNames = cache.Items
            .Where(x => !x.HasPrice)
            .Select(x => x.ItemUniqueName)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(x => x, StringComparer.Ordinal)
            .ToList();

        var batches = CreatePriceBatches(missingItemNames);
        for (var batchIndex = 0; batchIndex < batches.Count; batchIndex++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var batch = batches[batchIndex];
            var prices = await _apiClient.GetPricesAsync(serverLocation, batch, cancellationToken).ConfigureAwait(false);

            if (prices == null)
            {
                throw new InvalidOperationException("Albion Data Project prices could not be loaded.");
            }

            ApplyPrices(cache.Items, batch, prices);
            priceBatchLoaded?.Invoke(batchIndex + 1, batches.Count);
            await SaveCacheAsync(cache, cacheFilePath).ConfigureAwait(false);
        }

        if (refreshAllPrices)
        {
            cache.LastPriceSyncUtc = utcNow;
            await SaveCacheAsync(cache, cacheFilePath).ConfigureAwait(false);
        }
    }

    private static void ApplyPrices(
        IEnumerable<LossExplorerCachedItem> cachedItems,
        IReadOnlyCollection<string> requestedItemNames,
        IReadOnlyList<MarketResponse> prices)
    {
        var requestedNames = requestedItemNames.ToHashSet(StringComparer.Ordinal);
        var pricesByKey = prices
            .Where(x => x != null && !string.IsNullOrWhiteSpace(x.ItemTypeId))
            .GroupBy(x => CreateItemKey(x.ItemTypeId, x.QualityLevel), StringComparer.Ordinal)
            .ToDictionary(x => x.Key, x => x.ToList(), StringComparer.Ordinal);

        foreach (var cachedItem in cachedItems.Where(x => requestedNames.Contains(x.ItemUniqueName)))
        {
            var key = CreateItemKey(cachedItem.ItemUniqueName, cachedItem.QualityLevel);
            cachedItem.UnitValue = pricesByKey.TryGetValue(key, out var itemPrices)
                ? GetMedianMarketValue(itemPrices)
                : 0;
            cachedItem.HasPrice = true;
        }
    }

    private static ulong GetMedianMarketValue(IEnumerable<MarketResponse> prices)
    {
        var sellPrices = prices
            .Where(x => x.SellPriceMin > 0)
            .Select(x => x.SellPriceMin)
            .OrderBy(x => x)
            .ToList();

        if (sellPrices.Count > 0)
        {
            return Median(sellPrices);
        }

        var buyPrices = prices
            .Where(x => x.BuyPriceMax > 0)
            .Select(x => x.BuyPriceMax)
            .OrderBy(x => x)
            .ToList();
        return buyPrices.Count > 0 ? Median(buyPrices) : 0;
    }

    private static ulong Median(IReadOnlyList<ulong> values)
    {
        var middle = values.Count / 2;
        if (values.Count % 2 == 1)
        {
            return values[middle];
        }

        return (ulong) (((decimal) values[middle - 1] + values[middle]) / 2m);
    }

    private static List<List<string>> CreatePriceBatches(IEnumerable<string> itemUniqueNames)
    {
        var batches = new List<List<string>>();
        var currentBatch = new List<string>();
        var currentLength = 0;

        foreach (var itemUniqueName in itemUniqueNames)
        {
            var encodedLength = Uri.EscapeDataString(itemUniqueName).Length + (currentBatch.Count > 0 ? 1 : 0);
            if (currentBatch.Count > 0 && currentLength + encodedLength > PriceRequestMaximumLength)
            {
                batches.Add(currentBatch);
                currentBatch = [];
                currentLength = 0;
                encodedLength = Uri.EscapeDataString(itemUniqueName).Length;
            }

            currentBatch.Add(itemUniqueName);
            currentLength += encodedLength;
        }

        if (currentBatch.Count > 0)
        {
            batches.Add(currentBatch);
        }

        return batches;
    }

    private static string CreateItemKey(string itemUniqueName, int qualityLevel)
    {
        return $"{itemUniqueName}\u001f{qualityLevel}";
    }

    private static async Task SaveCacheAsync(LossExplorerCache cache, string cacheFilePath)
    {
        if (!await FileController.SaveAsync(cache, cacheFilePath, LossExplorerHistoryController.IsValidCache).ConfigureAwait(false))
        {
            Log.Warning("Loss Explorer cache could not be saved. File={File}", cacheFilePath);
        }
    }
}
