using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Models.BindingModel;
using System.Collections.ObjectModel;

namespace StatisticsAnalysisTool.ViewModels;

public class DashboardWindowViewModel : BaseViewModel
{
    public DashboardWindowViewModel(DashboardBindings dashboardBindings, ObservableCollection<MainStatObject> factionPointStats)
    {
        DashboardBindings = dashboardBindings;
        FactionPointStats = factionPointStats;
    }

    public DashboardBindings DashboardBindings
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<MainStatObject> FactionPointStats
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }
}