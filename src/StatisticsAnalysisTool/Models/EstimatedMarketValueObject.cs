using System;
using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Models;

public class EstimatedMarketValueObject
{
    [JsonPropertyName("Name")]
    public string UniqueItemName { get; set; }
    [JsonPropertyName("TS")]
    public DateTime Timestamp { get; set; }
    [JsonPropertyName("EstVal")]
    public long EstimatedMarketValueInternal { get; set; }
}