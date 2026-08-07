using StatisticsAnalysisTool.ViewModels;
using System.Collections.ObjectModel;

namespace StatisticsAnalysisTool.Models.BindingModel;

public class ItemDetailsQualityTabBindings : BaseViewModel
{
    private ObservableCollection<MainTabLocationFilterObject> _locationFilters;
    private ObservableCollection<MarketQualityObject> _prices = new();

    #region Bindings

    public ObservableCollection<MainTabLocationFilterObject> LocationFilters
    {
        get => _locationFilters;
        set
        {
            _locationFilters = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<MarketQualityObject> Prices
    {
        get => _prices;
        set
        {
            _prices = value;
            OnPropertyChanged();
        }
    }

    #endregion
}