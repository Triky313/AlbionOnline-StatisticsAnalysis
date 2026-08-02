using StatisticsAnalysisTool.Cluster;

namespace StatisticsAnalysisTool.Models;

public sealed class DashboardContentFilterOption
{
    public DashboardContentFilterOption(MapType? mapType, string name)
    {
        MapType = mapType;
        Name = name;
    }

    public MapType? MapType { get; }
    public string Name { get; }
}