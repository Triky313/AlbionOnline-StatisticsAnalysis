using System.Collections.Generic;

namespace StatisticsAnalysisTool.Trade;

public sealed class TradeProfitTimeOfDayResult
{
    public IReadOnlyList<TradeProfitTimeOfDayPoint> HeatmapPoints
    {
        get;
        init;
    } = [];

    public IReadOnlyList<TradeProfitTimeOfDayPoint> HourlyPoints
    {
        get;
        init;
    } = [];
}