using System.Windows.Media;

namespace StatisticsAnalysisTool.Gathering;

public sealed class GatheringResourceTypeSummary
{
    public GatheringResourceType ResourceType { get; init; }
    public string Name { get; init; } = string.Empty;
    public long Amount { get; init; }
    public long Value { get; init; }
    public double SharePercentage { get; init; }
    public Brush Brush { get; init; } = Brushes.Transparent;
}