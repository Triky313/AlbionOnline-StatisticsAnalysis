using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using StatisticsAnalysisTool.Localization;

namespace StatisticsAnalysisTool.Trade;

public sealed class TradeItemRankingService
{
    private readonly TradeAnalyticsValueService _tradeAnalyticsValueService = new();

    public TradeItemRankingResult BuildRankings(IEnumerable<Trade> trades, int maxEntries)
    {
        var tradeEntries = new List<TradeItemRankingEntry>();
        var aggregatedItemRankings = new Dictionary<string, TradeItemRankingAccumulator>(StringComparer.OrdinalIgnoreCase);

        foreach (var trade in trades ?? [])
        {
            if (trade == null)
            {
                continue;
            }

            AggregateTrade(aggregatedItemRankings, trade);

            if (TryCreateSingleTradeEntry(trade, out var tradeEntry))
            {
                tradeEntries.Add(tradeEntry);
            }
        }

        var itemRankings = aggregatedItemRankings
            .Values
            .Select(CreateEntryBase)
            .ToList();

        return new TradeItemRankingResult
        {
            TopItemsByProfit = BuildRanking(
                tradeEntries
                    .Where(x => x.NetProfit > 0d)
                    .OrderByDescending(x => x.NetProfit)
                    .ThenByDescending(x => x.SoldQuantity)
                    .ThenBy(x => x.ItemName, StringComparer.CurrentCultureIgnoreCase),
                maxEntries,
                entry => entry.NetProfit,
                entry => entry.NetProfitDisplay),
            TopItemsByLoss = BuildRanking(
                tradeEntries
                    .Where(x => x.NetProfit < 0d)
                    .OrderBy(x => x.NetProfit)
                    .ThenByDescending(x => x.BoughtQuantity)
                    .ThenBy(x => x.ItemName, StringComparer.CurrentCultureIgnoreCase),
                maxEntries,
                entry => entry.NetProfit,
                entry => entry.NetProfitDisplay),
            TopItemsByRoi = BuildRanking(
                itemRankings
                    .Where(x => x.BoughtQuantity > 0 && x.InvestedCapital > 0d)
                    .OrderByDescending(x => x.Roi)
                    .ThenByDescending(x => x.NetProfit)
                    .ThenBy(x => x.ItemName, StringComparer.CurrentCultureIgnoreCase),
                maxEntries,
                entry => entry.Roi,
                CreateRoiHighlightDisplay),
            TopSoldItemsByVolume = BuildRanking(
                itemRankings
                    .Where(x => x.SoldQuantity > 0)
                    .OrderByDescending(x => x.SoldQuantity)
                    .ThenByDescending(x => x.NetProfit)
                    .ThenBy(x => x.ItemName, StringComparer.CurrentCultureIgnoreCase),
                maxEntries,
                entry => entry.SoldQuantity,
                entry => entry.SoldQuantityDisplay),
            TopBoughtItemsByVolume = BuildRanking(
                itemRankings
                    .Where(x => x.BoughtQuantity > 0)
                    .OrderByDescending(x => x.BoughtQuantity)
                    .ThenByDescending(x => x.NetProfit)
                    .ThenBy(x => x.ItemName, StringComparer.CurrentCultureIgnoreCase),
                maxEntries,
                entry => entry.BoughtQuantity,
                entry => entry.BoughtQuantityDisplay)
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
        accumulator.BoughtQuantity += tradeBreakdown.BoughtQuantity;
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

    private bool TryCreateSingleTradeEntry(Trade trade, out TradeItemRankingEntry entry)
    {
        var itemKey = ResolveItemKey(trade);
        if (string.IsNullOrWhiteSpace(itemKey))
        {
            entry = new TradeItemRankingEntry();
            return false;
        }

        var tradeItemMetadata = ResolveItemMetadata(trade, itemKey);
        var tradeBreakdown = _tradeAnalyticsValueService.GetBreakdown(trade);
        var netProfit = tradeBreakdown.Sold - tradeBreakdown.Bought - tradeBreakdown.Tax;
        var investedCapital = tradeBreakdown.Bought + tradeBreakdown.Tax;
        var roi = investedCapital > 0d ? netProfit / investedCapital * 100d : 0d;

        entry = new TradeItemRankingEntry
        {
            ItemUniqueName = tradeItemMetadata.ItemUniqueName,
            ItemName = tradeItemMetadata.ItemName,
            TierLevelDisplay = tradeItemMetadata.TierLevelDisplay,
            NetProfit = netProfit,
            Roi = roi,
            InvestedCapital = investedCapital,
            SoldQuantity = tradeBreakdown.SoldQuantity,
            BoughtQuantity = tradeBreakdown.BoughtQuantity,
            TradeCount = 1
        };

        return true;
    }

    private static IReadOnlyList<TradeItemRankingEntry> BuildRanking(
        IEnumerable<TradeItemRankingEntry> entries,
        int maxEntries,
        Func<TradeItemRankingEntry, double> highlightNumericValueSelector,
        Func<TradeItemRankingEntry, string> highlightValueDisplaySelector)
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
                BoughtQuantity = entry.BoughtQuantity,
                TradeCount = entry.TradeCount,
                HighlightValue = highlightNumericValueSelector(entry),
                HighlightValueDisplay = highlightValueDisplaySelector(entry)
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
            BoughtQuantity = accumulator.BoughtQuantity,
            TradeCount = accumulator.TradeCount
        };
    }

    private static string CreateRoiHighlightDisplay(TradeItemRankingEntry entry)
    {
        var soldText = LocalizationController.Translation("SOLD").ToLower(CultureInfo.CurrentCulture);
        var boughtText = LocalizationController.Translation("BOUGHT").ToLower(CultureInfo.CurrentCulture);
        return $"{entry.RoiDisplay} ({entry.SoldQuantityDisplay} {soldText} | {entry.BoughtQuantityDisplay} {boughtText})";
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

        public long BoughtQuantity { get; set; }

        public int TradeCount { get; set; }
    }

    private readonly record struct TradeItemMetadata(string ItemUniqueName, string ItemName, string TierLevelDisplay);
}
