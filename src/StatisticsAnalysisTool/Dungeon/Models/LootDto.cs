using System;

namespace StatisticsAnalysisTool.Dungeon.Models;

public class LootDto
{
    public string UniqueName { get; set; }
    public DateTime UtcDiscoveryTime { get; set; }
    public int Quantity { get; set; }
    public long EstimatedMarketValueInternal { get; set; }
}