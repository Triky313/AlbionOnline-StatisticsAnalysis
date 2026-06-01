namespace StatisticsAnalysisTool.Crafting;

public class CraftingResourceResult
{
    public string UniqueName { get; set; }
    public CraftingResourceKind ResourceKind { get; set; }
    public bool IsReturnable { get; set; }
    public decimal QuantityPerRun { get; set; }
    public decimal GrossQuantity { get; set; }
    public decimal ExpectedReturnQuantity { get; set; }
    public decimal NetQuantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal GrossCost { get; set; }
    public decimal NetCost { get; set; }
    public double GrossWeight { get; set; }
    public double ExpectedReturnWeight { get; set; }
}