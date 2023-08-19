using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.ViewModels;

namespace StatisticsAnalysisTool.Models;

public class TabVisibilityFilter : BaseViewModel
{
    private bool? _isSelected;
    private string _name;

    public TabVisibilityFilter(NavigationTabFilterType navigationTabFilterType)
    {
        NavigationTabFilterType = navigationTabFilterType;
    }

    public NavigationTabFilterType NavigationTabFilterType { get; }

    public bool? IsSelected
    {
        get => _isSelected;
        set
        {
            _isSelected = value;
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
}