using System.Collections.Generic;

namespace StatisticsAnalysisTool.Trade;

public sealed class TradeProfitTimeSeriesResult
{
    public IReadOnlyList<TradeProfitTimeSeriesPoint> Points
    {
        get;
        init;
    } = [];

    public TradeProfitTimeAggregation RequestedAggregation
    {
        get;
        init;
    }

    public TradeProfitTimeAggregation EffectiveAggregation
    {
        get;
        init;
    }

    public int BucketStepSize
    {
        get;
        init;
    } = 1;
}
