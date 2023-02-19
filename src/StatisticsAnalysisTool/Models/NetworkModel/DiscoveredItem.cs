using System;
using System.Collections.Generic;
using StatisticsAnalysisTool.Common;

namespace StatisticsAnalysisTool.Models.NetworkModel;

public class DiscoveredItem
{
    public DiscoveredItem()
    {
        UtcDiscoveryTime = DateTime.UtcNow;
    }

    public long ObjectId { get; set; }
    public int ItemIndex { get; set; }
    public DateTime UtcDiscoveryTime { get; }
    public int Quantity { get; set; }
    public string BodyName { get; set; }
    public string LooterName { get; set; }
    public FixPoint CurrentDurability { get; set; }
    public long EstimatedMarketValueInternal { get; set; }
    public ItemQuality Quality { get; set; } = ItemQuality.Unknown;
    public Dictionary<int, int> SpellDictionary { get; set; }
}