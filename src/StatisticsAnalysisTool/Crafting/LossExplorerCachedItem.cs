namespace StatisticsAnalysisTool.Crafting;

public sealed class LossExplorerCachedItem
{
    public string ItemUniqueName { get; set; } = string.Empty;
    public int QualityLevel { get; set; }
    public decimal EquipmentQuantity { get; set; }
    public decimal InventoryQuantity { get; set; }
    public ulong UnitValue { get; set; }
    public bool HasPrice { get; set; }
}