namespace StatisticsAnalysisTool.Crafting;

public class CraftingResourceInput
{
    public string UniqueName { get; set; }
    public decimal QuantityPerRun { get; set; }
    public decimal UnitPrice { get; set; }
    public double UnitWeight { get; set; }
    public bool IsReturnable { get; set; } = true;
    public decimal? MaxReturnQuantityPerRun { get; set; }
    public CraftingResourceKind ResourceKind { get; set; } = CraftingResourceKind.Standard;
}