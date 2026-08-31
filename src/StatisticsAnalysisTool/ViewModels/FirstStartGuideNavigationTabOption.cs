using StatisticsAnalysisTool.Enumerations;

namespace StatisticsAnalysisTool.ViewModels;

public class FirstStartGuideNavigationTabOption(NavigationTabFilterType navigationTabFilterType) : BaseViewModel
{
    public NavigationTabFilterType NavigationTabFilterType { get; } = navigationTabFilterType;

    public string Name
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public bool IsSelected
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }
}