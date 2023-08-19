using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.ViewModels;
using System;

namespace StatisticsAnalysisTool.Models;

public class MainTabLocationFilterObject : BaseViewModel
{
    private bool? _isChecked;
    private string _name;

    public event Action OnCheckedChanged;

    public MainTabLocationFilterObject(MarketLocation location, string name, bool isChecked)
    {
        Location = location;
        Name = name;
        IsChecked = isChecked;
    }

    public MarketLocation Location { get; }

    public bool? IsChecked
    {
        get => _isChecked;
        set
        {
            _isChecked = value;
            OnCheckedChanged?.Invoke();
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