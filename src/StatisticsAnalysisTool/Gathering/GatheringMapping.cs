using StatisticsAnalysisTool.Common;

namespace StatisticsAnalysisTool.Gathering;

public static class GatheringMapping
{
    public static GatheredDto Mapping(Gathered gathered)
    {
        return new GatheredDto()
        {
            Timestamp = gathered.Timestamp,
            UniqueItemName = gathered.UniqueName,
            EstimatedMarketValueInternal = gathered.EstimatedMarketValue.InternalValue,
            GainedStandardAmount = gathered.GainedStandardAmount,
            GainedBonusAmount = gathered.GainedBonusAmount,
            GainedPremiumBonusAmount = gathered.GainedPremiumBonusAmount,
            GainedFame = gathered.GainedFame,
            ClusterIndex = gathered.ClusterIndex,
            MapType = gathered.MapType,
            InstanceName = gathered.InstanceName,
            MiningProcesses = gathered.MiningProcesses,
            HasBeenFished = gathered.HasBeenFished
        };
    }

    public static Gathered Mapping(GatheredDto gathered)
    {
        return new Gathered()
        {
            Timestamp = gathered.Timestamp,
            UniqueName = gathered.UniqueItemName,
            EstimatedMarketValue = FixPoint.FromInternalValue(gathered.EstimatedMarketValueInternal),
            GainedStandardAmount = gathered.GainedStandardAmount,
            GainedBonusAmount = gathered.GainedBonusAmount,
            GainedPremiumBonusAmount = gathered.GainedPremiumBonusAmount,
            GainedFame = gathered.GainedFame,
            ClusterIndex = gathered.ClusterIndex,
            MapType = gathered.MapType,
            InstanceName = gathered.InstanceName,
            MiningProcesses = gathered.MiningProcesses,
            HasBeenFished = gathered.HasBeenFished
        };
    }
}