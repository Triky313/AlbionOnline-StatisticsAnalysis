using StatisticsAnalysisTool.ViewModels;

namespace StatisticsAnalysisTool.Models;

public sealed class DashboardCombatEventItem(
    string relativeTime,
    string result,
    bool isPositiveResult,
    string mapName,
    string opponentName,
    DashboardCombatPlayerItem killer,
    DashboardCombatPlayerItem victim) : BaseViewModel
{
    public string RelativeTime { get; } = relativeTime;
    public string Result { get; } = result;
    public bool IsPositiveResult { get; } = isPositiveResult;
    public string MapName { get; } = mapName;
    public string OpponentName { get; } = opponentName;
    public DashboardCombatPlayerItem Killer { get; } = killer;
    public DashboardCombatPlayerItem Victim { get; } = victim;
    public double EstimatedValue => Victim.EstimatedValue;

    public bool IsExpanded
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }
}