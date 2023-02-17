using System;
using System.Text.Json.Serialization;
using StatisticsAnalysisTool.Common;

namespace StatisticsAnalysisTool.EstimatedMarketValue;

public class EstQualityValueDto
{
    [JsonPropertyName("TS")]
    public long Ticks { get; set; }
    [JsonPropertyName("MV")]
    public long MarketValueInternal { get; set; }
    [JsonPropertyName("Q")]
    public ItemQuality Quality { get; set; }
}