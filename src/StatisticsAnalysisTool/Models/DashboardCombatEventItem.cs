namespace StatisticsAnalysisTool.Models;

public sealed class DashboardCombatEventItem(
    string relativeTime,
    string result,
    bool isKill,
    string mapName,
    string opponentName,
    DashboardCombatPlayerItem killer,
    DashboardCombatPlayerItem victim)
{
    public string RelativeTime { get; } = relativeTime;
    public string Result { get; } = result;
    public bool IsKill { get; } = isKill;
    public string MapName { get; } = mapName;
    public string OpponentName { get; } = opponentName;
    public DashboardCombatPlayerItem Killer { get; } = killer;
    public DashboardCombatPlayerItem Victim { get; } = victim;
    public double EstimatedValue => Victim.EstimatedValue;
}