using StatisticsAnalysisTool.Models;

namespace StatisticsAnalysisTool.DamageMeter;

public sealed class DamageMeterContentFilterOption
{
    public DamageMeterContentFilterOption(DashboardContentType? contentType, string name)
    {
        ContentType = contentType;
        Name = name;
    }

    public DashboardContentType? ContentType { get; }
    public string Name { get; }
}