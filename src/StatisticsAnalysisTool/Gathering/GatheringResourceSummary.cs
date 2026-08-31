using StatisticsAnalysisTool.Models;

namespace StatisticsAnalysisTool.Gathering;

public sealed class GatheringResourceSummary
{
    public int Rank { get; init; }
    public string UniqueName { get; init; } = string.Empty;
    public Item Item { get; init; }
    public long TimesGathered { get; init; }
    public long TotalAmount { get; init; }
    public long TotalValue { get; init; }
    public double AverageValuePerGather { get; init; }
    public double GatheringDurationSeconds { get; init; }
    public string TopLocation { get; init; } = string.Empty;
}