namespace StatisticsAnalysisTool.Models.NetworkModel;

public class HarvestFinishedObject
{
    public long UserObjectId { get; init; }
    public long ObjectId { get; init; }
    public int ItemId { get; init; }
    public int StandardAmount { get; init; }
    public int CollectorBonusAmount { get; init; }
    public int PremiumBonusAmount { get; init; }
    public int CurrentPossibleDegradationProcesses { get; init; }
}