namespace StatisticsAnalysisTool.Dungeon.Models;

public sealed class DungeonEfficiencyEntry
{
    public string TierEnchantment { get; init; } = string.Empty;
    public double AverageLootPerRun { get; init; }
    public double AverageFamePerRun { get; init; }
    public double AverageDurationInSeconds { get; init; }
    public double LootScore { get; init; }
    public double FameScore { get; init; }
}