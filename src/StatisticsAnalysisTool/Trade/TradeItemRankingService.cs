using System;
using System.Collections.Generic;
using System.Linq;

namespace StatisticsAnalysisTool.Trade;

public sealed class TradeItemRankingService
{
    private readonly TradeAnalyticsValueService _tradeAnalyticsValueService = new();

    public TradeItemRankingResult BuildRankings(IEnumerable<Trade> trades, int maxEntries)
    {
        var itemRankings = (trades ?? [])
            .Where(x => x != null)
            .Aggregate(
                new Dictionary<string, TradeItemRankingAccumulator>(StringComparer.OrdinalIgnoreCase),
                AggregateTrade)
            .Values
            .Select(CreateEntryBase)
            .ToList();

        return new TradeItemRankingResult
        {
            TopItemsByProfit = BuildRanking(
                itemRankings
                    .Where(x => x.NetProfit != 0d)
                    .OrderByDescending(x => x.NetProfit)
                    .ThenByDescending(x => x.SoldQuantity)
                    .ThenBy(x => x.ItemName, StringComparer.CurrentCultureIgnoreCase),
                maxEntries,
                entry => entry.NetProfitDisplay),
            TopItemsByRoi = BuildRanking(
                itemRankings
                    .Where(x => x.InvestedCapital > 0d)
                    .OrderByDescending(x => x.Roi)
                    .ThenByDescending(x => x.NetProfit)
                    .ThenBy(x => x.ItemName, StringComparer.CurrentCultureIgnoreCase),
                maxEntries,
                entry => entry.RoiDisplay),
            TopItemsByVolume = BuildRanking(
                itemRankings
                    .Where(x => x.SoldQuantity > 0)
                    .OrderByDescending(x => x.SoldQuantity)
                    .ThenByDescending(x => x.NetProfit)
                    .ThenBy(x => x.ItemName, StringComparer.CurrentCultureIgnoreCase),
                maxEntries,
                entry => entry.SoldQuantityDisplay)
        };
    }

    private Dictionary<string, TradeItemRankingAccumulator> AggregateTrade(Dictionary<string, TradeItemRankingAccumulator> rankings, Trade trade)
    {
        var itemKey = ResolveItemKey(trade);
        if (string.IsNullOrWhiteSpace(itemKey))
        {
            return rankings;
        }

        var tradeItemMetadata = ResolveItemMetadata(trade, itemKey);

        if (!rankings.TryGetValue(itemKey, out var accumulator))
        {
            accumulator = new TradeItemRankingAccumulator(
                tradeItemMetadata.ItemUniqueName,
                tradeItemMetadata.ItemName,
                tradeItemMetadata.TierLevelDisplay);
            rankings[itemKey] = accumulator;
        }

        var tradeBreakdown = _tradeAnalyticsValueService.GetBreakdown(trade);
        accumulator.Sold += tradeBreakdown.Sold;
        accumulator.Bought += tradeBreakdown.Bought;
        accumulator.Tax += tradeBreakdown.Tax;
        accumulator.SoldQuantity += tradeBreakdown.SoldQuantity;
        accumulator.TradeCount++;

        if (string.IsNullOrWhiteSpace(accumulator.ItemName))
        {
            accumulator.ItemName = tradeItemMetadata.ItemName;
        }

        if (string.IsNullOrWhiteSpace(accumulator.TierLevelDisplay))
        {
            accumulator.TierLevelDisplay = tradeItemMetadata.TierLevelDisplay;
        }

        if (string.IsNullOrWhiteSpace(accumulator.ItemUniqueName))
        {
            accumulator.ItemUniqueName = tradeItemMetadata.ItemUniqueName;
        }

        return rankings;
    }

    private static IReadOnlyList<TradeItemRankingEntry> BuildRanking(
        IEnumerable<TradeItemRankingEntry> entries,
        int maxEntries,
        Func<TradeItemRankingEntry, string> highlightValueSelector)
    {
        return entries
            .Take(Math.Max(1, maxEntries))
            .Select((entry, index) => new TradeItemRankingEntry
            {
                Rank = index + 1,
                ItemUniqueName = entry.ItemUniqueName,
                ItemName = entry.ItemName,
                TierLevelDisplay = entry.TierLevelDisplay,
                NetProfit = entry.NetProfit,
                Roi = entry.Roi,
                InvestedCapital = entry.InvestedCapital,
                SoldQuantity = entry.SoldQuantity,
                TradeCount = entry.TradeCount,
                HighlightValueDisplay = highlightValueSelector(entry)
            })
            .ToList();
    }

    private static TradeItemRankingEntry CreateEntryBase(TradeItemRankingAccumulator accumulator)
    {
        var netProfit = accumulator.Sold - accumulator.Bought - accumulator.Tax;
        var investedCapital = accumulator.Bought + accumulator.Tax;
        var roi = investedCapital > 0d ? netProfit / investedCapital * 100d : 0d;

        return new TradeItemRankingEntry
        {
            ItemUniqueName = accumulator.ItemUniqueName,
            ItemName = accumulator.ItemName,
            TierLevelDisplay = accumulator.TierLevelDisplay,
            NetProfit = netProfit,
            Roi = roi,
            InvestedCapital = investedCapital,
            SoldQuantity = accumulator.SoldQuantity,
            TradeCount = accumulator.TradeCount
        };
    }

    private static string ResolveItemKey(Trade trade)
    {
        return trade.Item?.UniqueName
               ?? trade.MailContent?.UniqueItemName
               ?? trade.AuctionEntry?.ItemTypeId
               ?? string.Empty;
    }

    private static TradeItemMetadata ResolveItemMetadata(Trade trade, string itemKey)
    {
        if (trade.Item != null)
        {
            return new TradeItemMetadata(
                trade.Item.UniqueName,
                trade.Item.LocalizedName,
                trade.Item.TierLevelString);
        }

        var itemName = string.IsNullOrWhiteSpace(itemKey) ? string.Empty : itemKey;
        return new TradeItemMetadata(itemKey, itemName, string.Empty);
    }

    private sealed class TradeItemRankingAccumulator(string itemUniqueName, string itemName, string tierLevelDisplay)
    {
        public string ItemUniqueName { get; set; } = itemUniqueName;

        public string ItemName { get; set; } = itemName;

        public string TierLevelDisplay { get; set; } = tierLevelDisplay;

        public double Sold { get; set; }

        public double Bought { get; set; }

        public double Tax { get; set; }

        public long SoldQuantity { get; set; }

        public int TradeCount { get; set; }
    }

    private readonly record struct TradeItemMetadata(string ItemUniqueName, string ItemName, string TierLevelDisplay);
}