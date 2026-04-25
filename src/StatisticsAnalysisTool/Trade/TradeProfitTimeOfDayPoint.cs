using System;

namespace StatisticsAnalysisTool.Trade;

public sealed class TradeProfitTimeOfDayPoint
{
    public int Hour { get; init; }
    public DayOfWeek? DayOfWeek { get; init; }
    public double Sold { get; init; }
    public double Bought { get; init; }
    public double Tax { get; init; }
    public double NetProfit { get; init; }
    public double AverageNetProfitPerTrade { get; init; }
    public int TradeCount { get; init; }

    public double GetMetricValue(TradeTimeOfDayMetric metric)
    {
        return metric switch
        {
            TradeTimeOfDayMetric.NetProfit => NetProfit,
            TradeTimeOfDayMetric.AverageProfitPerTrade => AverageNetProfitPerTrade,
            TradeTimeOfDayMetric.TradeCount => TradeCount,
            _ => NetProfit
        };
    }
}