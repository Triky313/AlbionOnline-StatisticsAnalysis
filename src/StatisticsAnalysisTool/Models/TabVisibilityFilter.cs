using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Properties;
using StatisticsAnalysisTool.ViewModels;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace StatisticsAnalysisTool.Models;

public class TabVisibilityFilter : INotifyPropertyChanged
{
    private bool? _isSelected;
    private string _name;

    public TabVisibilityFilter(NavigationTabFilterType navigationTabFilterType)
    {
        NavigationTabFilterType = navigationTabFilterType;
    }

    public NavigationTabFilterType NavigationTabFilterType { get; }

    private void SetFilter()
    {
        var mainWindowViewModel = ServiceLocator.Resolve<MainWindowViewModel>();

        switch (NavigationTabFilterType)
        {
            case NavigationTabFilterType.Dashboard:
                mainWindowViewModel.DashboardTabVisibility = IsSelected.BoolToVisibility();
                break;
            case NavigationTabFilterType.ItemSearch:
                mainWindowViewModel.ItemSearchTabVisibility = IsSelected.BoolToVisibility();
                break;
            case NavigationTabFilterType.Logging:
                mainWindowViewModel.LoggingTabVisibility = IsSelected.BoolToVisibility();
                break;
            case NavigationTabFilterType.Dungeons:
                mainWindowViewModel.DungeonsTabVisibility = IsSelected.BoolToVisibility();
                break;
            case NavigationTabFilterType.DamageMeter:
                mainWindowViewModel.DamageMeterTabVisibility = IsSelected.BoolToVisibility();
                break;
            case NavigationTabFilterType.TradeMonitoring:
                mainWindowViewModel.TradeMonitoringTabVisibility = IsSelected.BoolToVisibility();
                break;
            case NavigationTabFilterType.Gathering:
                mainWindowViewModel.GatheringTabVisibility = IsSelected.BoolToVisibility();
                break;
            case NavigationTabFilterType.StorageHistory:
                mainWindowViewModel.StorageHistoryTabVisibility = IsSelected.BoolToVisibility();
                break;
            case NavigationTabFilterType.MapHistory:
                mainWindowViewModel.MapHistoryTabVisibility = IsSelected.BoolToVisibility();
                break;
            case NavigationTabFilterType.PlayerInformation:
                mainWindowViewModel.PlayerInformationTabVisibility = IsSelected.BoolToVisibility();
                break;
        }
    }

    public bool? IsSelected
    {
        get => _isSelected;
        set
        {
            _isSelected = value;
            SetFilter();
            OnPropertyChanged();
        }
    }

    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}