using System;

namespace StatisticsAnalysisTool.Crafting;

public sealed class LossExplorerDailyItem
{
    public DateOnly Day { get; set; }
    public string ItemUniqueName { get; set; } = string.Empty;
    public int QualityLevel { get; set; }
    public long EquipmentQuantity { get; set; }
    public long InventoryQuantity { get; set; }
}