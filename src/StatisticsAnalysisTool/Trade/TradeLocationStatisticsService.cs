using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Localization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StatisticsAnalysisTool.Trade;

public sealed class TradeLocationStatisticsService
{
    private readonly TradeAnalyticsValueService _tradeAnalyticsValueService = new();

    public static IReadOnlyList<KeyValuePair<MarketLocation, string>> SupportedMarketLocations { get; } = BuildSupportedMarketLocations();

    public IReadOnlyList<TradeLocationStatisticsEntry> Build(IEnumerable<Trade> trades, IReadOnlySet<MarketLocation> selectedLocations)
    {
        var availableLocations = SupportedMarketLocations
            .Where(location => selectedLocations.Count == 0 || selectedLocations.Contains(location.Key))
            .ToArray();
        var accumulators = availableLocations.ToDictionary(
            location => location.Key,
            location => new TradeLocationStatisticsAccumulator(location.Key, location.Value));

        foreach (var trade in trades ?? [])
        {
            if (trade == null || !accumulators.TryGetValue(trade.Location, out var accumulator))
            {
                continue;
            }

            var breakdown = _tradeAnalyticsValueService.GetBreakdown(trade);
            var isSale = breakdown.Sold > 0d || breakdown.SoldQuantity > 0;
            var isPurchase = breakdown.Bought > 0d || breakdown.BoughtQuantity > 0;

            if (!isSale && !isPurchase)
            {
                continue;
            }

            if (isSale)
            {
                accumulator.SalesCount++;
                accumulator.SalesValue += breakdown.Sold;
            }

            if (isPurchase)
            {
                accumulator.PurchasesCount++;
                accumulator.PurchasesValue += breakdown.Bought;
            }

            accumulator.TaxPaid += breakdown.Tax;
            accumulator.AddCategory(trade.Item?.FullItemInformation?.ShopCategory, isSale, isPurchase);
        }

        return accumulators.Values
            .Select(CreateEntry)
            .OrderByDescending(entry => entry.SalesCount + entry.PurchasesCount)
            .ThenBy(entry => entry.LocationName, StringComparer.CurrentCultureIgnoreCase)
            .ToArray();
    }

    private static IReadOnlyList<KeyValuePair<MarketLocation, string>> BuildSupportedMarketLocations()
    {
        var locations = Locations.OnceMarketLocations
            .Where(location => location.Key != MarketLocation.SmugglersDen)
            .ToList();

        locations.Add(new KeyValuePair<MarketLocation, string>(MarketLocation.ArthursRest, Locations.GetDisplayName(MarketLocation.ArthursRest)));
        locations.Add(new KeyValuePair<MarketLocation, string>(MarketLocation.MerlynsRest, Locations.GetDisplayName(MarketLocation.MerlynsRest)));
        locations.Add(new KeyValuePair<MarketLocation, string>(MarketLocation.MorganasRest, Locations.GetDisplayName(MarketLocation.MorganasRest)));
        locations.Add(Locations.OnceMarketLocations.First(location => location.Key == MarketLocation.SmugglersDen));

        return locations;
    }

    private static TradeLocationStatisticsEntry CreateEntry(TradeLocationStatisticsAccumulator accumulator)
    {
        var netProfit = accumulator.SalesValue - accumulator.PurchasesValue - accumulator.TaxPaid;
        var margin = accumulator.SalesValue <= 0d ? 0d : netProfit / accumulator.SalesValue * 100d;

        return new TradeLocationStatisticsEntry
        {
            Location = accumulator.Location,
            LocationName = accumulator.LocationName,
            SalesCount = accumulator.SalesCount,
            SalesValue = accumulator.SalesValue,
            PurchasesCount = accumulator.PurchasesCount,
            PurchasesValue = accumulator.PurchasesValue,
            NetProfit = netProfit,
            Margin = margin,
            TaxPaid = accumulator.TaxPaid,
            MostTradedCategory = GetLocalizedCategoryName(accumulator.GetMostTradedCategory())
        };
    }

    private static string GetLocalizedCategoryName(string categoryId)
    {
        if (string.IsNullOrWhiteSpace(categoryId))
        {
            return "—";
        }

        var translationKey = $"@MARKETPLACEGUI_ROLLOUT_SHOPCATEGORY_{categoryId.ToUpperInvariant()}";
        var translatedCategory = LocalizationController.Translation(translationKey);
        return string.Equals(translatedCategory, translationKey, StringComparison.OrdinalIgnoreCase)
            ? categoryId
            : translatedCategory;
    }

    private sealed class TradeLocationStatisticsAccumulator(MarketLocation location, string locationName)
    {
        private readonly Dictionary<string, int> _categoryCounts = new(StringComparer.OrdinalIgnoreCase);

        public MarketLocation Location { get; } = location;

        public string LocationName { get; } = locationName ?? string.Empty;

        public int SalesCount { get; set; }

        public double SalesValue { get; set; }

        public int PurchasesCount { get; set; }

        public double PurchasesValue { get; set; }

        public double TaxPaid { get; set; }

        public void AddCategory(string categoryId, bool isSale, bool isPurchase)
        {
            if (string.IsNullOrWhiteSpace(categoryId))
            {
                return;
            }

            var activityCount = (isSale ? 1 : 0) + (isPurchase ? 1 : 0);
            _categoryCounts[categoryId] = _categoryCounts.GetValueOrDefault(categoryId) + activityCount;
        }

        public string GetMostTradedCategory()
        {
            return _categoryCounts
                .OrderByDescending(category => category.Value)
                .ThenBy(category => category.Key, StringComparer.OrdinalIgnoreCase)
                .Select(category => category.Key)
                .FirstOrDefault() ?? string.Empty;
        }
    }
}
