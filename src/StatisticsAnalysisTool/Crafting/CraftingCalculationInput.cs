using System.Collections.Generic;

namespace StatisticsAnalysisTool.Crafting;

public class CraftingCalculationInput
{
    public string ItemUniqueName
    {
        get;
        set;
    }

    public int CraftingRuns
    {
        get;
        set;
    }
    = 1;

    public int AmountCrafted
    {
        get;
        set;
    }
    = 1;

    public decimal ReturnRatePercent
    {
        get;
        set;
    }

    public bool UsesFocus
    {
        get;
        set;
    }

    public decimal OutputUnitPrice
    {
        get;
        set;
    }

    public decimal StationFee
    {
        get;
        set;
    }

    public decimal NutritionConsumedPerRun
    {
        get;
        set;
    }

    public decimal SalesTaxPercent
    {
        get;
        set;
    }

    public decimal SetupFeePercent
    {
        get;
        set;
    }
    = 2.5m;

    public decimal OtherCosts
    {
        get;
        set;
    }

    public double OutputUnitWeight
    {
        get;
        set;
    }

    public List<CraftingResourceInput> Resources
    {
        get;
        set;
    }
    = [];

    public CraftingJournalInput Journal
    {
        get;
        set;
    }
}
