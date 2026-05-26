using StatisticsAnalysisTool.Common;
using System;

namespace StatisticsAnalysisTool.Crafting;

public sealed class BlackMarketAverageStatsRequestContext
{
    public int RequestId { get; init; }

    public string ItemUniqueName { get; init; }

    public int ItemIndex { get; init; }

    public int QualityLevel { get; init; }

    public int TimeRange { get; init; }

    public MarketLocation MarketLocation { get; init; }

    public DateTime RequestedUtc { get; init; } = DateTime.UtcNow;
}