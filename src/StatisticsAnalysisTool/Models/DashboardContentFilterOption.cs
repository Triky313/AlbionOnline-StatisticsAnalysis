namespace StatisticsAnalysisTool.Models;

public sealed class DashboardContentFilterOption
{
    public DashboardContentFilterOption(DashboardContentType? contentType, string name)
    {
        ContentType = contentType;
        Name = name;
    }

    public DashboardContentType? ContentType { get; }
    public string Name { get; }
}