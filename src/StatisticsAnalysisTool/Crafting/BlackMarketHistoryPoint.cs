using System;

namespace StatisticsAnalysisTool.Crafting;

public class BlackMarketHistoryPoint
{
    public DateTime Date { get; set; }

    public int ItemCount { get; set; }

    public long AveragePrice { get; set; }

    public DateTime LastUpdatedUtc { get; set; }
}