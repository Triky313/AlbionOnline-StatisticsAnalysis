namespace StatisticsAnalysisTool.Models;

public sealed class DashboardLootBreakdownItem
{
    public DashboardLootBreakdownItem(string name, long itemCount, double sharePercentage)
    {
        Name = name;
        ItemCount = itemCount;
        SharePercentage = sharePercentage;
    }

    public string Name { get; }
    public long ItemCount { get; }
    public double SharePercentage { get; }
}