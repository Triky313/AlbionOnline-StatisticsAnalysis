using System;
using StatisticsAnalysisTool.Enumerations;

namespace StatisticsAnalysisTool.Dungeon.Models;

public class LootDto
{
    public string UniqueName { get; set; }
    public DateTime UtcDiscoveryTime { get; set; }
    public int Quantity { get; set; }
    public long EstimatedMarketValueInternal { get; set; }
    public long SourceObjectId { get; set; }
    public string SourceName { get; set; } = string.Empty;
    public DungeonLootSourceType SourceType { get; set; }
}