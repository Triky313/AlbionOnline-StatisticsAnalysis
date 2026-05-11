using System.Collections.Generic;

namespace StatisticsAnalysisTool.Crafting;

public class CraftingCalculationResult
{
    public int CraftingRuns { get; set; }
    public int AmountCrafted { get; set; }
    public int OutputQuantity { get; set; }
    public decimal ReturnRatePercent { get; set; }
    public bool UsesFocus { get; set; }
    public List<CraftingResourceResult> Resources { get; set; } = [];
    public CraftingJournalResult Journal { get; set; }
    public decimal GrossMaterialCosts { get; set; }
    public decimal NetMaterialCosts { get; set; }
    public decimal NonReturnableMaterialCosts { get; set; }
    public decimal StationFee { get; set; }
    public decimal SalesTax { get; set; }
    public decimal OtherCosts { get; set; }
    public decimal JournalCosts { get; set; }
    public decimal JournalRevenue { get; set; }
    public decimal SalesRevenueGross { get; set; }
    public decimal SalesRevenueNet { get; set; }
    public decimal TotalCosts { get; set; }
    public decimal Profit { get; set; }
    public decimal RoiPercent { get; set; }
    public decimal BreakEvenPrice { get; set; }
    public decimal ProfitPerItem { get; set; }
    public double WeightBeforeCrafting { get; set; }
    public double WeightAfterCrafting { get; set; }
}