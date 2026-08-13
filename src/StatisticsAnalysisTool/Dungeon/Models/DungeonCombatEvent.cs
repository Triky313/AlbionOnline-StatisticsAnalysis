using System.Windows;

namespace StatisticsAnalysisTool.Dungeon.Models;

public sealed class DungeonCombatEvent
{
    public DungeonCombatEvent(KillStatus status, string diedName, string killedBy)
    {
        Status = status;
        DiedName = diedName;
        KilledBy = killedBy;
    }

    public KillStatus Status { get; }
    public string DiedName { get; }
    public string KilledBy { get; }
    public Visibility KilledByVisibility => string.IsNullOrWhiteSpace(KilledBy) ? Visibility.Collapsed : Visibility.Visible;
}