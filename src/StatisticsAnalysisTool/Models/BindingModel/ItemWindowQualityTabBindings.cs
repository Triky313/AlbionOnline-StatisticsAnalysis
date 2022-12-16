using StatisticsAnalysisTool.Properties;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace StatisticsAnalysisTool.Models.BindingModel;

public class ItemWindowQualityTabBindings : INotifyPropertyChanged
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

    public event PropertyChangedEventHandler PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}