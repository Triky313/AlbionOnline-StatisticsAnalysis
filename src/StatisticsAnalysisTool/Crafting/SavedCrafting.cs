using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Windows.Media.Imaging;

namespace StatisticsAnalysisTool.Crafting;

public class SavedCrafting : BaseViewModel
{
    public Guid Id
    {
        get;
        set;
    }
    = Guid.NewGuid();

    public string ItemUniqueName { get; set; }

    [JsonIgnore]
    public string ItemName => ItemController.GetItemByUniqueName(ItemUniqueName)?.LocalizedName ?? ItemUniqueName ?? string.Empty;

    public int CraftingRuns { get; set; } = 1;
    public int AmountCrafted { get; set; } = 1;
    public bool UsesFocus { get; set; }
    public decimal ReturnRatePercent { get; set; }
    public decimal DailyBonusPercent { get; set; }
    public int HideoutBonusLevel { get; set; }
    public decimal HideoutGeneralistBonusPercent { get; set; }
    public decimal HideoutSpecialistBonusPercent { get; set; }
    public string CraftingLocationId { get; set; }
    public string CraftingLocationName { get; set; }
    public string CraftingContext { get; set; }
    public decimal StationFee { get; set; }
    public decimal SalesTaxPercent { get; set; } = 4.0m;
    public decimal SetupFeePercent { get; set; } = 2.5m;
    public decimal OtherCosts { get; set; }
    public decimal OutputUnitPrice { get; set; }
    public string Notes
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }
    public List<CraftingResourceEntry> Resources { get; set; } = [];
    public CraftingJournalEntry Journal { get; set; }
    public DateTime LastChangedUtc { get; set; } = DateTime.UtcNow;
    public decimal NetMaterialCosts { get; set; }
    public decimal TotalCosts { get; set; }
    public decimal Profit { get; set; }
    public decimal ProfitPerItem { get; set; }
    public decimal RoiPercent { get; set; }
    public decimal BreakEvenPrice { get; set; }
    [JsonIgnore]
    public BitmapImage Icon { get; set; }

    [JsonIgnore]
    public bool IsProfitPositive => Profit > 0m;

    [JsonIgnore]
    public bool IsProfitNegative => Profit < 0m;

    [JsonIgnore]
    public bool IsProfitPerItemPositive => ProfitPerItem > 0m;

    [JsonIgnore]
    public bool IsProfitPerItemNegative => ProfitPerItem < 0m;

    [JsonIgnore]
    public bool IsRoiPositive => RoiPercent > 0m;

    [JsonIgnore]
    public bool IsRoiNegative => RoiPercent < 0m;

    [JsonIgnore]
    public bool IsBreakEvenPricePositive => BreakEvenPrice > 0m;

    [JsonIgnore]
    public bool IsBreakEvenPriceNegative => BreakEvenPrice < 0m;

    [JsonIgnore]
    public string Summary => CraftingRuns
                             + " "
                             + LocalizationController.Translation("RUNS")
                             + " | "
                             + ReturnRatePercent.ToString("N2")
                             + "% RRR"
                             + (DailyBonusPercent <= 0m
                                 ? string.Empty
                                 : " | " + LocalizationController.Translation("DAILY") + " " + DailyBonusPercent.ToString("N0") + "%")
                             + (HideoutBonusLevel <= 0
                                 ? string.Empty
                                 : " | " + LocalizationController.Translation("HIDEOUT") + " L" + HideoutBonusLevel)
                             + (string.IsNullOrWhiteSpace(CraftingLocationName)
                                 ? string.Empty
                                 : " | " + CraftingLocationName);
}
