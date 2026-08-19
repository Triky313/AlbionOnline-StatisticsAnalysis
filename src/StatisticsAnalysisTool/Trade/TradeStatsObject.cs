using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.ViewModels;
using StatisticsAnalysisTool.Models;
using System;
using System.Collections.Generic;

namespace StatisticsAnalysisTool.Trade;

public class TradeStatsObject : BaseViewModel
{
    private long _soldToday;
    private long _soldMonth;
    private long _soldYear;
    private long _boughtToday;
    private long _boughtMonth;
    private long _boughtYear;
    private long _soldTotal;
    private long _boughtTotal;
    private long _salesToday;
    private long _salesMonth;
    private long _salesYear;
    private long _salesTotal;
    private long _soldThisWeek;
    private long _boughtThisWeek;
    private long _salesThisWeek;
    private long _soldLastWeek;
    private long _boughtLastWeek;
    private long _salesLastWeek;
    private long _taxesToday;
    private long _taxesThisWeek;
    private long _taxesLastWeek;
    private long _taxesMonth;
    private long _taxesYear;
    private long _taxesTotal;
    private long _soldLastMonth;
    private long _boughtLastMonth;
    private long _taxesLastMonth;
    private long _salesLastMonth;
    private Trade _mostExpensiveSaleItem;
    private Trade _mostExpensivePurchasedItem;

    #region Stat calculations

    public void SetTradeStats(IEnumerable<Trade> trades)
    {
        var currentStatistics = CalculateTradeStatistics(trades, DateTime.UtcNow);
        ApplyTradeStatistics(currentStatistics);
        UpdateSummaryMetrics(TradePeriodTotals.Empty);
    }

    public void SetTradeStats(List<Trade> trades, List<Trade> previousPeriodTrades)
    {
        var currentUtc = DateTime.UtcNow;
        var currentStatistics = CalculateTradeStatistics(trades, currentUtc);
        var previousTotals = CalculateTradeTotals(previousPeriodTrades);

        ApplyTradeStatistics(currentStatistics);
        UpdateSummaryMetrics(previousTotals);
    }

    private void ApplyTradeStatistics(TradeStatisticsResult statistics)
    {
        SoldToday = statistics.Today.Sold;
        SoldThisWeek = statistics.ThisWeek.Sold;
        SoldLastWeek = statistics.LastWeek.Sold;
        SoldMonth = statistics.Month.Sold;
        SoldLastMonth = statistics.LastMonth.Sold;
        SoldYear = statistics.Year.Sold;
        SoldTotal = statistics.Total.Sold;

        BoughtToday = statistics.Today.Bought;
        BoughtThisWeek = statistics.ThisWeek.Bought;
        BoughtLastWeek = statistics.LastWeek.Bought;
        BoughtMonth = statistics.Month.Bought;
        BoughtLastMonth = statistics.LastMonth.Bought;
        BoughtYear = statistics.Year.Bought;
        BoughtTotal = statistics.Total.Bought;

        TaxesToday = statistics.Today.Taxes;
        TaxesThisWeek = statistics.ThisWeek.Taxes;
        TaxesLastWeek = statistics.LastWeek.Taxes;
        TaxesMonth = statistics.Month.Taxes;
        TaxesLastMonth = statistics.LastMonth.Taxes;
        TaxesYear = statistics.Year.Taxes;
        TaxesTotal = statistics.Total.Taxes;

        SalesToday = SoldToday - BoughtToday - TaxesToday;
        SalesThisWeek = SoldThisWeek - BoughtThisWeek - TaxesThisWeek;
        SalesLastWeek = SoldLastWeek - BoughtLastWeek - TaxesLastWeek;
        SalesMonth = SoldMonth - BoughtMonth - TaxesMonth;
        SalesLastMonth = SoldLastMonth - BoughtLastMonth - TaxesLastMonth;
        SalesYear = SoldYear - BoughtYear - TaxesYear;
        SalesTotal = SoldTotal - BoughtTotal - TaxesTotal;

        MostExpensiveSaleItem = statistics.MostExpensiveSaleItem;
        MostExpensivePurchasedItem = statistics.MostExpensivePurchasedItem;
        PeriodStatistics = BuildPeriodStatistics();
    }

    public void RefreshLocalization()
    {
        OnPropertyChanged(nameof(TranslationSold));
        OnPropertyChanged(nameof(TranslationToday));
        OnPropertyChanged(nameof(TranslationThisWeek));
        OnPropertyChanged(nameof(TranslationLastWeek));
        OnPropertyChanged(nameof(TranslationMonth));
        OnPropertyChanged(nameof(TranslationLastMonth));
        OnPropertyChanged(nameof(TranslationYear));
        OnPropertyChanged(nameof(TranslationTotal));
        OnPropertyChanged(nameof(TranslationBought));
        OnPropertyChanged(nameof(TranslationTax));
        OnPropertyChanged(nameof(TranslationNetProfit));
        OnPropertyChanged(nameof(TranslationMostExpensiveSale));
        OnPropertyChanged(nameof(TranslationMostExpensivePurchase));
        OnPropertyChanged(nameof(TranslationSilver));
        OnPropertyChanged(nameof(TranslationTotalSales));
        OnPropertyChanged(nameof(TranslationTotalPurchases));
        OnPropertyChanged(nameof(TranslationTradeVolume));
        OnPropertyChanged(nameof(TranslationProfit));
        OnPropertyChanged(nameof(TranslationTradeStatistics));
        OnPropertyChanged(nameof(TranslationPreviousPeriod));
        PeriodStatistics = BuildPeriodStatistics();
    }

    private void UpdateSummaryMetrics(TradePeriodTotals previousTotals)
    {
        var previousVolume = previousTotals.Sold + previousTotals.Bought;
        var previousProfit = previousTotals.Sold - previousTotals.Bought - previousTotals.Taxes;

        TotalSalesSummary.Update(SoldTotal, SoldTotal, previousTotals.Sold);
        TotalPurchasesSummary.Update(BoughtTotal, BoughtTotal, previousTotals.Bought);
        TradeVolumeSummary.Update(SoldTotal + BoughtTotal, SoldTotal + BoughtTotal, previousVolume);
        ProfitSummary.Update(SalesTotal, SalesTotal, previousProfit);
    }

    private IReadOnlyList<TradePeriodStatisticsEntry> BuildPeriodStatistics()
    {
        return
        [
            new(TranslationToday, SoldToday, BoughtToday, TaxesToday, SalesToday),
            new(TranslationThisWeek, SoldThisWeek, BoughtThisWeek, TaxesThisWeek, SalesThisWeek),
            new(TranslationLastWeek, SoldLastWeek, BoughtLastWeek, TaxesLastWeek, SalesLastWeek),
            new(TranslationMonth, SoldMonth, BoughtMonth, TaxesMonth, SalesMonth),
            new(TranslationLastMonth, SoldLastMonth, BoughtLastMonth, TaxesLastMonth, SalesLastMonth),
            new(TranslationYear, SoldYear, BoughtYear, TaxesYear, SalesYear),
            new(TranslationTotal, SoldTotal, BoughtTotal, TaxesTotal, SalesTotal, true)
        ];
    }

    private static TradeStatisticsResult CalculateTradeStatistics(IEnumerable<Trade> trades, DateTime currentUtc)
    {
        var today = TradePeriodTotals.Empty;
        var thisWeek = TradePeriodTotals.Empty;
        var lastWeek = TradePeriodTotals.Empty;
        var month = TradePeriodTotals.Empty;
        var lastMonth = TradePeriodTotals.Empty;
        var year = TradePeriodTotals.Empty;
        var total = TradePeriodTotals.Empty;
        Trade mostExpensiveSaleItem = null;
        Trade mostExpensivePurchasedItem = null;
        long mostExpensiveSaleValue = 0;
        long mostExpensivePurchaseValue = 0;
        var currentDate = currentUtc.Date;
        var lastWeekDate = currentUtc.AddDays(-7);
        var lastMonthDate = currentUtc.AddMonths(-1);

        foreach (var trade in trades)
        {
            if (trade == null)
            {
                continue;
            }

            var values = GetTradeStatisticsValues(trade);
            var tradeDate = trade.Timestamp.Date;
            total = total.Add(values);

            if (tradeDate == currentDate)
            {
                today = today.Add(values);
            }

            if (tradeDate.IsDateInWeekOfYear(currentUtc))
            {
                thisWeek = thisWeek.Add(values);
            }

            if (tradeDate.IsDateInWeekOfYear(lastWeekDate))
            {
                lastWeek = lastWeek.Add(values);
            }

            if (tradeDate.Year == currentUtc.Year && tradeDate.Month == currentUtc.Month)
            {
                month = month.Add(values);
            }

            if (tradeDate.IsDateInSameMonth(lastMonthDate))
            {
                lastMonth = lastMonth.Add(values);
            }

            if (tradeDate.Year == currentUtc.Year)
            {
                year = year.Add(values);
            }

            if (TryGetSaleValue(trade, out var saleValue)
                && (mostExpensiveSaleItem == null || saleValue > mostExpensiveSaleValue))
            {
                mostExpensiveSaleItem = trade;
                mostExpensiveSaleValue = saleValue;
            }

            if (TryGetPurchaseValue(trade, out var purchaseValue)
                && (mostExpensivePurchasedItem == null || purchaseValue > mostExpensivePurchaseValue))
            {
                mostExpensivePurchasedItem = trade;
                mostExpensivePurchaseValue = purchaseValue;
            }
        }

        return new TradeStatisticsResult(
            today,
            thisWeek,
            lastWeek,
            month,
            lastMonth,
            year,
            total,
            mostExpensiveSaleItem,
            mostExpensivePurchasedItem);
    }

    private static TradePeriodTotals CalculateTradeTotals(IEnumerable<Trade> trades)
    {
        var totals = TradePeriodTotals.Empty;
        foreach (var trade in trades)
        {
            if (trade != null)
            {
                totals = totals.Add(GetTradeStatisticsValues(trade));
            }
        }

        return totals;
    }

    private static TradeStatisticsValues GetTradeStatisticsValues(Trade trade)
    {
        return trade.Type switch
        {
            TradeType.Mail => GetMailTradeStatisticsValues(trade),
            TradeType.InstantSell => new TradeStatisticsValues(
                trade.InstantBuySellContent.TotalPrice.IntegerValue,
                0,
                trade.InstantBuySellContent.TaxPrice.IntegerValue),
            TradeType.ManualSell => new TradeStatisticsValues(
                trade.InstantBuySellContent.TotalPrice.IntegerValue,
                0,
                0),
            TradeType.PlayerTradeIncoming => new TradeStatisticsValues(
                trade.PlayerTradeContent.IsSilver ? trade.PlayerTradeContent.Silver.IntegerValue : 0,
                0,
                0),
            TradeType.InstantBuy or TradeType.ManualBuy or TradeType.Crafting => new TradeStatisticsValues(
                0,
                trade.InstantBuySellContent.TotalPrice.IntegerValue,
                0),
            TradeType.PlayerTradeOutgoing => new TradeStatisticsValues(
                0,
                trade.PlayerTradeContent.IsSilver ? trade.PlayerTradeContent.Silver.IntegerValue : 0,
                0),
            _ => TradeStatisticsValues.Empty
        };
    }

    private static TradeStatisticsValues GetMailTradeStatisticsValues(Trade trade)
    {
        var taxes = trade.MailContent.TaxSetupPrice.IntegerValue + trade.MailContent.TaxPrice.IntegerValue;
        return trade.MailType switch
        {
            MailType.MarketplaceSellOrderFinished or MailType.MarketplaceSellOrderExpired => new TradeStatisticsValues(
                trade.MailContent.TotalPrice.IntegerValue,
                0,
                taxes),
            MailType.MarketplaceBuyOrderFinished or MailType.MarketplaceBuyOrderExpired => new TradeStatisticsValues(
                0,
                trade.MailContent.TotalPrice.IntegerValue,
                taxes),
            _ => TradeStatisticsValues.Empty
        };
    }

    private static bool TryGetSaleValue(Trade trade, out long value)
    {
        switch (trade.Type)
        {
            case TradeType.Mail when trade.MailType is MailType.MarketplaceSellOrderFinished or MailType.MarketplaceSellOrderExpired:
                value = trade.MailContent.TotalPrice.IntegerValue;
                return true;
            case TradeType.InstantSell:
                value = trade.InstantBuySellContent.TotalPrice.IntegerValue;
                return true;
            default:
                value = 0;
                return false;
        }
    }

    private static bool TryGetPurchaseValue(Trade trade, out long value)
    {
        switch (trade.Type)
        {
            case TradeType.Mail when trade.MailType is MailType.MarketplaceBuyOrderFinished or MailType.MarketplaceBuyOrderExpired:
                value = trade.MailContent.TotalPrice.IntegerValue;
                return true;
            case TradeType.InstantBuy:
                value = trade.InstantBuySellContent.TotalPrice.IntegerValue;
                return true;
            default:
                value = 0;
                return false;
        }
    }

    private readonly record struct TradeStatisticsValues(long Sold, long Bought, long Taxes)
    {
        public static TradeStatisticsValues Empty => new(0, 0, 0);
    }

    private readonly record struct TradePeriodTotals(long Sold, long Bought, long Taxes)
    {
        public static TradePeriodTotals Empty => new(0, 0, 0);

        public TradePeriodTotals Add(TradeStatisticsValues values)
        {
            return new TradePeriodTotals(Sold + values.Sold, Bought + values.Bought, Taxes + values.Taxes);
        }
    }

    private readonly record struct TradeStatisticsResult(
        TradePeriodTotals Today,
        TradePeriodTotals ThisWeek,
        TradePeriodTotals LastWeek,
        TradePeriodTotals Month,
        TradePeriodTotals LastMonth,
        TradePeriodTotals Year,
        TradePeriodTotals Total,
        Trade MostExpensiveSaleItem,
        Trade MostExpensivePurchasedItem);

    #endregion

    public long SoldToday
    {
        get => _soldToday;
        set
        {
            _soldToday = value;
            OnPropertyChanged();
        }
    }

    public long SoldThisWeek
    {
        get => _soldThisWeek;
        set
        {
            _soldThisWeek = value;
            OnPropertyChanged();
        }
    }

    public long SoldLastWeek
    {
        get => _soldLastWeek;
        set
        {
            _soldLastWeek = value;
            OnPropertyChanged();
        }
    }

    public long SoldMonth
    {
        get => _soldMonth;
        set
        {
            _soldMonth = value;
            OnPropertyChanged();
        }
    }

    public long SoldLastMonth
    {
        get => _soldLastMonth;
        set
        {
            _soldLastMonth = value;
            OnPropertyChanged();
        }
    }

    public long SoldYear
    {
        get => _soldYear;
        set
        {
            _soldYear = value;
            OnPropertyChanged();
        }
    }

    public long BoughtToday
    {
        get => _boughtToday;
        set
        {
            _boughtToday = value;
            OnPropertyChanged();
        }
    }

    public long BoughtThisWeek
    {
        get => _boughtThisWeek;
        set
        {
            _boughtThisWeek = value;
            OnPropertyChanged();
        }
    }

    public long BoughtLastWeek
    {
        get => _boughtLastWeek;
        set
        {
            _boughtLastWeek = value;
            OnPropertyChanged();
        }
    }

    public long BoughtMonth
    {
        get => _boughtMonth;
        set
        {
            _boughtMonth = value;
            OnPropertyChanged();
        }
    }

    public long BoughtLastMonth
    {
        get => _boughtLastMonth;
        set
        {
            _boughtLastMonth = value;
            OnPropertyChanged();
        }
    }

    public long BoughtYear
    {
        get => _boughtYear;
        set
        {
            _boughtYear = value;
            OnPropertyChanged();
        }
    }

    public long SoldTotal
    {
        get => _soldTotal;
        set
        {
            _soldTotal = value;
            OnPropertyChanged();
        }
    }

    public long BoughtTotal
    {
        get => _boughtTotal;
        set
        {
            _boughtTotal = value;
            OnPropertyChanged();
        }
    }

    public long SalesToday
    {
        get => _salesToday;
        set
        {
            _salesToday = value;
            OnPropertyChanged();
        }
    }

    public long SalesThisWeek
    {
        get => _salesThisWeek;
        set
        {
            _salesThisWeek = value;
            OnPropertyChanged();
        }
    }

    public long SalesLastWeek
    {
        get => _salesLastWeek;
        set
        {
            _salesLastWeek = value;
            OnPropertyChanged();
        }
    }

    public long SalesMonth
    {
        get => _salesMonth;
        set
        {
            _salesMonth = value;
            OnPropertyChanged();
        }
    }

    public long SalesLastMonth
    {
        get => _salesLastMonth;
        set
        {
            _salesLastMonth = value;
            OnPropertyChanged();
        }
    }

    public long SalesYear
    {
        get => _salesYear;
        set
        {
            _salesYear = value;
            OnPropertyChanged();
        }
    }

    public long SalesTotal
    {
        get => _salesTotal;
        set
        {
            _salesTotal = value;
            OnPropertyChanged();
        }
    }

    public long TaxesToday
    {
        get => _taxesToday;
        set
        {
            _taxesToday = value;
            OnPropertyChanged();
        }
    }

    public long TaxesThisWeek
    {
        get => _taxesThisWeek;
        set
        {
            _taxesThisWeek = value;
            OnPropertyChanged();
        }
    }

    public long TaxesLastWeek
    {
        get => _taxesLastWeek;
        set
        {
            _taxesLastWeek = value;
            OnPropertyChanged();
        }
    }

    public long TaxesMonth
    {
        get => _taxesMonth;
        set
        {
            _taxesMonth = value;
            OnPropertyChanged();
        }
    }

    public long TaxesLastMonth
    {
        get => _taxesLastMonth;
        set
        {
            _taxesLastMonth = value;
            OnPropertyChanged();
        }
    }

    public long TaxesYear
    {
        get => _taxesYear;
        set
        {
            _taxesYear = value;
            OnPropertyChanged();
        }
    }

    public long TaxesTotal
    {
        get => _taxesTotal;
        set
        {
            _taxesTotal = value;
            OnPropertyChanged();
        }
    }

    public Trade MostExpensiveSaleItem
    {
        get => _mostExpensiveSaleItem;
        set
        {
            _mostExpensiveSaleItem = value;
            OnPropertyChanged();
        }
    }

    public Trade MostExpensivePurchasedItem
    {
        get => _mostExpensivePurchasedItem;
        set
        {
            _mostExpensivePurchasedItem = value;
            OnPropertyChanged();
        }
    }

    public DashboardSummaryMetric TotalSalesSummary { get; } = new();
    public DashboardSummaryMetric TotalPurchasesSummary { get; } = new();
    public DashboardSummaryMetric TradeVolumeSummary { get; } = new();
    public DashboardSummaryMetric ProfitSummary { get; } = new();

    public IReadOnlyList<TradePeriodStatisticsEntry> PeriodStatistics
    {
        get;
        private set
        {
            field = value;
            OnPropertyChanged();
        }
    } = [];

    public string TranslationSold => LocalizationController.Translation("SOLD");
    public string TranslationToday => LocalizationController.Translation("TODAY");
    public string TranslationThisWeek => LocalizationController.Translation("THIS_WEEK");
    public string TranslationLastWeek => LocalizationController.Translation("LAST_WEEK");
    public string TranslationMonth => LocalizationController.Translation("MONTH");
    public string TranslationLastMonth => LocalizationController.Translation("LAST_MONTH");
    public string TranslationYear => LocalizationController.Translation("YEAR");
    public string TranslationTotal => LocalizationController.Translation("TOTAL");
    public string TranslationBought => LocalizationController.Translation("BOUGHT");
    public string TranslationTax => LocalizationController.Translation("TAX");
    public string TranslationNetProfit => LocalizationController.Translation("NET_PROFIT");
    public string TranslationMostExpensiveSale => LocalizationController.Translation("MOST_EXPENSIVE_SALE");
    public string TranslationMostExpensivePurchase => LocalizationController.Translation("MOST_EXPENSIVE_PURCHASE");

    public string TranslationSilver => LocalizationController.Translation("SILVER");
    public string TranslationTotalSales => LocalizationController.Translation("TOTAL_SALES");
    public string TranslationTotalPurchases => LocalizationController.Translation("TOTAL_PURCHASES");
    public string TranslationTradeVolume => LocalizationController.Translation("TRADE_VOLUME");
    public string TranslationProfit => LocalizationController.Translation("PROFIT");
    public string TranslationTradeStatistics => LocalizationController.Translation("TRADE_STATISTICS");
    public string TranslationPreviousPeriod => LocalizationController.Translation("VS_PREVIOUS_PERIOD");
}
