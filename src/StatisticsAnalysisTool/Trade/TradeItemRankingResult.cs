using System.Collections.Generic;

namespace StatisticsAnalysisTool.Trade;

public sealed class TradeItemRankingResult
{
    public IReadOnlyList<TradeItemRankingEntry> TopItemsByProfit
    {
        get;
        init;
    } = [];

    public IReadOnlyList<TradeItemRankingEntry> TopItemsByRoi
    {
        get;
        init;
    } = [];

    public IReadOnlyList<TradeItemRankingEntry> TopItemsByVolume
    {
        get;
        init;
    } = [];
}