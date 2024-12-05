using System;

namespace StatisticsAnalysisTool.EventLogging;

public class VaultContainerLogItem
{
    public DateTime Timestamp { get; set; }
    public string PlayerName { get; set; }
    public string LocalizedName { get; set; }
    public int Enchantment { get; set; }
    public int Quality { get; set; }
    public int Quantity { get; set; }
}