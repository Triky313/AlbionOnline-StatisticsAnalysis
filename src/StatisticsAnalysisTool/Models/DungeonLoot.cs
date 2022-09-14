using StatisticsAnalysisTool.Common;
using System;
using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Models;

public class DungeonLoot
{
    public string UniqueName { get; set; }
    public DateTime UtcDiscoveryTime { get; set; }
    public int Quantity { get; set; }
    public long EstimatedMarketValueInternal { get; set; }
    [JsonIgnore] 
    public FixPoint EstimatedMarketValue => FixPoint.FromInternalValue(EstimatedMarketValueInternal);
    [JsonIgnore]
    public string Hash => $"{UniqueName}{UtcDiscoveryTime}{Quantity}{EstimatedMarketValueInternal}";
}