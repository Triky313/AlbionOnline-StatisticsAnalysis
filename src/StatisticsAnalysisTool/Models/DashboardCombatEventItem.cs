namespace StatisticsAnalysisTool.Models;

public sealed class DashboardCombatEventItem(
    string relativeTime,
    string result,
    bool isKill,
    string mapName,
    string opponentName,
    double estimatedValue)
{
    public string RelativeTime { get; } = relativeTime;
    public string Result { get; } = result;
    public bool IsKill { get; } = isKill;
    public string MapName { get; } = mapName;
    public string OpponentName { get; } = opponentName;
    public double EstimatedValue { get; } = estimatedValue;
}