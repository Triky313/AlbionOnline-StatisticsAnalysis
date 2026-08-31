namespace StatisticsAnalysisTool.Cluster;

public static class MistsRarityResolver
{
    public static MistsRarity FromValue(int value)
    {
        return value switch
        {
            0 => MistsRarity.Common,
            1 => MistsRarity.Uncommon,
            2 => MistsRarity.Rare,
            3 => MistsRarity.Epic,
            4 => MistsRarity.Legendary,
            _ => MistsRarity.Unknown
        };
    }
}