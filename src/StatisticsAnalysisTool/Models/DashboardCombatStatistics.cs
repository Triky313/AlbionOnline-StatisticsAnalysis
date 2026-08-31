using StatisticsAnalysisTool.ViewModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace StatisticsAnalysisTool.Models;

public sealed class DashboardCombatStatistics : BaseViewModel
{
    public long KillCount
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public long DeathCount
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public long KnockoutCount
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public long KnockedOutCount
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public double KillDeathRatio
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public double TotalKillLootValue
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public double TotalDeathLootValue
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<DashboardCombatLocationItem> TopKillLocations { get; } = [];
    public ObservableCollection<DashboardCombatLocationItem> TopDeathLocations { get; } = [];

    public IReadOnlyList<DashboardCombatEventItem> RecentEvents
    {
        get;
        private set
        {
            field = value;
            OnPropertyChanged();
        }
    } = [];

    public void ReplaceRecentEvents(IReadOnlyList<DashboardCombatEventItem> recentEvents)
    {
        RecentEvents = recentEvents;
    }
}