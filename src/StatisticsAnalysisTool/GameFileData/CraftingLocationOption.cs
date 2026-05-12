using StatisticsAnalysisTool.Localization;

namespace StatisticsAnalysisTool.GameFileData;

public class CraftingLocationOption
{
    public string ClusterId { get; init; }
    public string DisplayName { get; init; }
    public string ClusterType { get; init; }
    public decimal BaseCraftingBonusPercent { get; init; }
    public decimal MatchingModifierPercent { get; init; }

    public decimal TotalProductionBonusPercent => BaseCraftingBonusPercent + MatchingModifierPercent;

    public decimal ExpectedReturnRatePercent => CraftingLocationData.GetExpectedReturnRatePercent(TotalProductionBonusPercent);

    public string DisplayText => string.IsNullOrWhiteSpace(DisplayName) ? ClusterId : DisplayName + " (" + ExpectedReturnRatePercent.ToString("N2") + "% RRR)";

    public string BonusSummary => LocalizationController.Translation("BONUS")
                                  + " "
                                  + TotalProductionBonusPercent.ToString("N2")
                                  + "% | "
                                  + LocalizationController.Translation("EXPECTED_RRR")
                                  + " "
                                  + ExpectedReturnRatePercent.ToString("N2")
                                  + "%";
}