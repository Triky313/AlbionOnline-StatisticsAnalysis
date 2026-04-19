using System;

namespace StatisticsAnalysisTool.Models.NetworkModel;

public class Loot
{
    public int ItemIndex { get; set; }
    public DateTime UtcPickupTime { get; } = DateTime.UtcNow;
    public int Quantity { get; set; }
    public string LootedFromName { get; set; }
    public string LootedByName { get; set; }
    public bool IsSilver { get; set; }
    public bool IsTrash { get; set; }
}