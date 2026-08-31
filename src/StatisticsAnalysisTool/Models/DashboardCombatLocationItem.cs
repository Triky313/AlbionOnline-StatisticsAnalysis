using StatisticsAnalysisTool.Cluster;

namespace StatisticsAnalysisTool.Models;

public sealed class DashboardCombatLocationItem(
    string name,
    long count,
    double sharePercentage,
    double barPercentage,
    double estimatedLootValue,
    ClusterType clusterType)
{
    public string Name { get; } = name;
    public long Count { get; } = count;
    public double SharePercentage { get; } = sharePercentage;
    public double BarPercentage { get; } = barPercentage;
    public double EstimatedLootValue { get; } = estimatedLootValue;
    public ClusterType ClusterType { get; } = clusterType;

    public string IconSource => ClusterType switch
    {
        ClusterType.SafeArea => "/Assets/map_blue_icon.png",
        ClusterType.Yellow => "/Assets/map_yellow_icon.png",
        ClusterType.Red => "/Assets/map_red_icon.png",
        ClusterType.Black => "/Assets/map_black_icon.png",
        ClusterType.Corrupted => "/Assets/map_orange_icon.png",
        _ => "/Assets/map_white_icon.png"
    };
}