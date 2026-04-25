using System;

namespace StatisticsAnalysisTool.Trade;

public sealed class TradeProfitTimeSeriesPoint
{
    public DateTime PeriodStart { get; init; }
    public DateTime PeriodEnd { get; init; }
    public double Sold { get; init; }
    public double Bought { get; init; }
    public double Tax { get; init; }
    public double NetProfit { get; init; }
    public double CumulativeNetProfit { get; init; }
    public double AverageProfitPerTrade { get; init; }
    public int TradeCount { get; init; }
}