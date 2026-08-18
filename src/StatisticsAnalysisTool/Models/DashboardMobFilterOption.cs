namespace StatisticsAnalysisTool.Models;

public sealed class DashboardMobFilterOption
{
    public DashboardMobFilterOption(string value, string name)
    {
        Value = value ?? string.Empty;
        Name = name ?? string.Empty;
    }

    public string Value { get; }
    public string Name { get; }
}