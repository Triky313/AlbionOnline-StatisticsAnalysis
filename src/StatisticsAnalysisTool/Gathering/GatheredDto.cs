namespace StatisticsAnalysisTool.Gathering;

public class GatheredDto
{
    public long Timestamp { get; init; }
    public long ObjectId { get; init; }
    public string UniqueItemName { get; init; }
    public int GainedStandardAmount { get; init; }
    public int GainedBonusAmount { get; init; }
    public int GainedPremiumBonusAmount { get; init; }
    public int GainedFame { get; init; }
    public string ClusterIndex { get; init; }
}