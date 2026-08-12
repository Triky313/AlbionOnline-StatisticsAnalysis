using StatisticsAnalysisTool.Enumerations;

namespace StatisticsAnalysisTool.Dungeon.Models;

public sealed class DungeonLootSource
{
    public static DungeonLootSource Unknown { get; } = new();

    public long ObjectId { get; init; }
    public string Name { get; init; } = string.Empty;
    public DungeonLootSourceType Type { get; init; }
}