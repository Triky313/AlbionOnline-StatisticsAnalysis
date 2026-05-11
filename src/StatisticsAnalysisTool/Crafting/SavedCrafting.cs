using StatisticsAnalysisTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Windows.Media.Imaging;

namespace StatisticsAnalysisTool.Crafting;

public class SavedCrafting : BaseViewModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string ItemUniqueName { get; set; }
    public string ItemName { get; set; }
    public int CraftingRuns { get; set; } = 1;
    public int AmountCrafted { get; set; } = 1;
    public bool UsesFocus { get; set; }
    public decimal ReturnRatePercent { get; set; }
    public string CraftingContext { get; set; }
    public decimal StationFee { get; set; }
    public decimal SalesTaxPercent { get; set; }
    public decimal OtherCosts { get; set; }
    public decimal OutputUnitPrice { get; set; }
    public string Notes { get; set; }
    public List<CraftingResourceEntry> Resources { get; set; } = [];
    public CraftingJournalEntry Journal { get; set; }
    public DateTime LastChangedUtc { get; set; } = DateTime.UtcNow;
    public decimal NetMaterialCosts { get; set; }
    public decimal Profit { get; set; }
    [JsonIgnore]
    public BitmapImage Icon { get; set; }
    [JsonIgnore]
    public bool IsProfitNegative => Profit < 0m;

    [JsonIgnore]
    public string Summary => CraftingRuns
                             + " runs | "
                             + (UsesFocus ? "Focus" : "No focus")
                             + " | "
                             + ReturnRatePercent.ToString("N2")
                             + "% RRR";
}