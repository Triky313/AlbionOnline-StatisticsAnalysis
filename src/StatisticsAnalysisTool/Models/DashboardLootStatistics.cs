using StatisticsAnalysisTool.ViewModels;
using System.Collections.ObjectModel;

namespace StatisticsAnalysisTool.Models;

public sealed class DashboardLootStatistics : BaseViewModel
{
    public DashboardSummaryMetric TotalValueSummary { get; } = new();
    public ObservableCollection<DashboardLootItem> RecentItems { get; } = [];
    public ObservableCollection<DashboardLootItem> MostValuableItems { get; } = [];
    public ObservableCollection<DashboardLootAreaItem> TopAreas { get; } = [];
    public ObservableCollection<DashboardLootBreakdownItem> ValueDistribution { get; } = [];
    public ObservableCollection<DashboardLootBreakdownItem> TierDistribution { get; } = [];
    public ObservableCollection<DashboardLootBreakdownItem> EnchantmentDistribution { get; } = [];

    public double AverageValue
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public double LootPerHour
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }
}