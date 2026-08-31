using StatisticsAnalysisTool.Enumerations;

namespace StatisticsAnalysisTool.ViewModels;

public class FirstStartGuideServerOption(ServerLocation serverLocation) : BaseViewModel
{
    public ServerLocation ServerLocation { get; } = serverLocation;

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