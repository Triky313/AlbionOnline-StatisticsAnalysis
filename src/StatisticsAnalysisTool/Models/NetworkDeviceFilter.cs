using StatisticsAnalysisTool.ViewModels;

namespace StatisticsAnalysisTool.Models;

public class NetworkDeviceFilter : BaseViewModel
{
    private bool? _isSelected;
    private string _name = string.Empty;

    public string Identifier { get; set; } = string.Empty;
    public int Index { get; set; } = -1;

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