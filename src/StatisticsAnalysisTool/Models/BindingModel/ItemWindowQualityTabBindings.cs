using StatisticsAnalysisTool.ViewModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace StatisticsAnalysisTool.Models.BindingModel;

public class ItemWindowQualityTabBindings : BaseViewModel
{
    private ObservableCollection<MainTabLocationFilterObject> _locationFilters;
    private List<MarketQualityObject> _prices = new();

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

    public List<MarketQualityObject> Prices
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