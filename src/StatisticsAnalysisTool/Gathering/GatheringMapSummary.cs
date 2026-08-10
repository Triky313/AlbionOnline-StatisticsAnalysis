using StatisticsAnalysisTool.Cluster;
using StatisticsAnalysisTool.Models;
using System.Windows.Media;

namespace StatisticsAnalysisTool.Gathering;

public sealed class GatheringMapSummary
{
    public string Name { get; init; } = string.Empty;
    public string ClusterIndex { get; init; } = string.Empty;
    public long TimesGathered { get; init; }
    public long TotalValue { get; init; }
    public int ResourceTypeCount { get; init; }
    public double GatheringDurationSeconds { get; init; }
    public ClusterType ClusterType { get; init; }
    public Item MostGatheredResource { get; init; }
    public Brush Brush { get; init; } = Brushes.Transparent;

    public string IconSource => ClusterType switch
    {
        ClusterType.SafeArea => "/Assets/map_blue_icon.png",
        ClusterType.Yellow => "/Assets/map_yellow_icon.png",
        ClusterType.Red => "/Assets/map_red_icon.png",
        ClusterType.Black => "/Assets/map_black_icon.png",
        _ => "/Assets/map_white_icon.png"
    };
}