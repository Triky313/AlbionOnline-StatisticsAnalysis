using System;
using StatisticsAnalysisTool.Common;

namespace StatisticsAnalysisTool.EstimatedMarketValue;

public class EstQualityValue
{
    public DateTime Timestamp { get; set; }
    public FixPoint MarketValue { get; set; }
    public ItemQuality Quality { get; set; }
}