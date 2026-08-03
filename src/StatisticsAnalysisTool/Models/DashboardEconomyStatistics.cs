namespace StatisticsAnalysisTool.Models;

public sealed class DashboardEconomyStatistics
{
    public double ReSpec { get; internal set; }
    public double ReSpecSilverCost { get; internal set; }
    public double SpentReSpec { get; internal set; }
    public double RepairCosts { get; internal set; }
    public double HighestRepairCost { get; internal set; }
}