namespace StatisticsAnalysisTool.Models;

public sealed class DashboardEconomyStatistics
{
    public double ReSpec { get; internal set; }
    public double ReSpecSilverCost { get; internal set; }
    public double SpentReSpec { get; internal set; }
    public double RepairCosts { get; internal set; }
    public double HighestRepairCost { get; internal set; }
    public double ItemQualityRerollCosts { get; internal set; }
    public int NormalItemCount { get; internal set; }
    public int GoodItemCount { get; internal set; }
    public int OutstandingItemCount { get; internal set; }
    public int ExcellentItemCount { get; internal set; }
    public int MasterpieceItemCount { get; internal set; }
    public int GoodItemSuccessfulRerollCount { get; internal set; }
    public int OutstandingItemSuccessfulRerollCount { get; internal set; }
    public int ExcellentItemSuccessfulRerollCount { get; internal set; }
    public int MasterpieceItemSuccessfulRerollCount { get; internal set; }
    public int GoodItemEligibleRerollCount { get; internal set; }
    public int OutstandingItemEligibleRerollCount { get; internal set; }
    public int ExcellentItemEligibleRerollCount { get; internal set; }
    public int MasterpieceItemEligibleRerollCount { get; internal set; }
}