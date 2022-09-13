using StatisticsAnalysisTool.Enumerations;

namespace StatisticsAnalysisTool.Models;

public struct DungeonStatsFilterStruct
{
    public string Name { get; set; }
    public DungeonStatTimeType DungeonStatTimeType { get; set; }
}