using System;
using System.Collections.Generic;
using System.Linq;

namespace StatisticsAnalysisTool.Crafting;

public class BlackMarketHistoryEntry
{
    public int FormatVersion { get; set; }

    public string ItemUniqueName { get; set; }

    public int ItemIndex { get; set; }

    public int Tier { get; set; }

    public int EnchantmentLevel { get; set; }

    public int QualityLevel { get; set; }

    public ulong CurrentBuyPrice { get; set; }

    public DateTime CurrentBuyPriceDateUtc { get; set; }

    public DateTime LastUpdatedUtc { get; set; }

    public List<BlackMarketHistoryPoint> Points { get; set; } = [];

    public long LatestAveragePrice => Points
        .OrderByDescending(x => x.Date)
        .FirstOrDefault()?.AveragePrice ?? 0;

    public int SoldLast30Days => GetSoldItems(DateTime.UtcNow.Date.AddDays(-29));

    public int SoldLast365Days => GetSoldItems(DateTime.UtcNow.Date.AddDays(-364));

    private int GetSoldItems(DateTime fromDate)
    {
        return Points
            .Where(x => x.Date.Date >= fromDate.Date)
            .Sum(x => x.ItemCount);
    }
}