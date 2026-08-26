using StatisticsAnalysisTool.Cluster;
using System;

namespace StatisticsAnalysisTool.Gathering;

public class GatheredDto
{
    public long Timestamp { get; init; }
    public Guid SessionId { get; init; }
    public string CharacterName { get; init; } = string.Empty;
    public long ObjectId { get; init; }
    public string UniqueItemName { get; init; }
    public int GainedStandardAmount { get; init; }
    public int GainedBonusAmount { get; init; }
    public int GainedPremiumBonusAmount { get; init; }
    public int GainedFame { get; init; }
    public int MiningProcesses { get; init; }
    public string ClusterIndex { get; init; }
    public MapType MapType { get; init; }
    public string InstanceName { get; init; }
    public bool HasBeenFished { get; init; }
    public long EstimatedMarketValueInternal { get; init; }
}