using StatisticsAnalysisTool.Enumerations;

namespace StatisticsAnalysisTool.ViewModels;

public class FirstStartGuideServerOption : BaseViewModel
{
    private bool _isSelected;
    private string _name;

    public FirstStartGuideServerOption(ServerLocation serverLocation)
    {
        ServerLocation = serverLocation;
    }

    public ServerLocation ServerLocation { get; }

    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            OnPropertyChanged();
        }
    }

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            _isSelected = value;
            OnPropertyChanged();
        }
    }
}