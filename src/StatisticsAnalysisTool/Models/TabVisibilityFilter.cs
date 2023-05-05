using StatisticsAnalysisTool.Common;
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

    public event PropertyChangedEventHandler PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}