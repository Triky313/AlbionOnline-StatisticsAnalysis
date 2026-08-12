using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Models;
using System;
using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Dungeon.Models;

public class Loot
{
    public string UniqueName { get; set; }
    public DateTime UtcDiscoveryTime { get; set; }
    public int Quantity { get; set; }
    public long EstimatedMarketValueInternal { get; set; }
    public long SourceObjectId { get; set; }
    public string SourceName { get; set; } = string.Empty;
    public DungeonLootSourceType SourceType { get; set; }
    [JsonIgnore]
    public Item Item => ItemController.GetItemByUniqueName(UniqueName);
    [JsonIgnore]
    public FixPoint EstimatedMarketValue => FixPoint.FromInternalValue(EstimatedMarketValueInternal);
    [JsonIgnore]
    public string Hash => $"{UniqueName}{UtcDiscoveryTime.Ticks}{Quantity}{EstimatedMarketValueInternal}{SourceObjectId}{SourceType}";
}