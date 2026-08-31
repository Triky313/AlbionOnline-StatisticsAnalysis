namespace StatisticsAnalysisTool.Dungeon.Models;

public sealed class DungeonCombatEventDto
{
    public KillStatus Status { get; set; }
    public string DiedName { get; set; }
    public string KilledBy { get; set; }
}