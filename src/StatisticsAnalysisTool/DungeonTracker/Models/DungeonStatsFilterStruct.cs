using StatisticsAnalysisTool.Enumerations;

namespace StatisticsAnalysisTool.DungeonTracker.Models;

public struct DungeonStatsFilterStruct
{
    public string Name { get; set; }
    public DungeonStatTimeType DungeonStatTimeType { get; set; }
}