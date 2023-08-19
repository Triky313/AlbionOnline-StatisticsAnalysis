using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Models.BindingModel;
using System.Collections.ObjectModel;

namespace StatisticsAnalysisTool.ViewModels;

public class DashboardWindowViewModel : BaseViewModel
{
    private DashboardBindings _dashboardBindings;
    private ObservableCollection<MainStatObject> _factionPointStats;

    public DashboardWindowViewModel(DashboardBindings dashboardBindings, ObservableCollection<MainStatObject> factionPointStats)
    {
        DashboardBindings = dashboardBindings;
        FactionPointStats = factionPointStats;
    }

    public DashboardBindings DashboardBindings
    {
        get => _dashboardBindings;
        set
        {
            _dashboardBindings = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<MainStatObject> FactionPointStats
    {
        get => _factionPointStats;
        set
        {
            _factionPointStats = value;
            OnPropertyChanged();
        }
    }
}