using StatisticsAnalysisTool.Enumerations;

namespace StatisticsAnalysisTool.Dungeon.Models;

public class CheckPointDto
{
    public int Id { get; set; }
    public CheckPointStatus Status { get; set; }
}