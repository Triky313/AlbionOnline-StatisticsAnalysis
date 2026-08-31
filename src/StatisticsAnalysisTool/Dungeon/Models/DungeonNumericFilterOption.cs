using StatisticsAnalysisTool.ViewModels;

namespace StatisticsAnalysisTool.Dungeon.Models;

public sealed class DungeonNumericFilterOption : BaseViewModel
{
    private string _displayName;
    private bool _isSelected;

    public DungeonNumericFilterOption(int value, string displayName, bool isAllOption, bool isSelected = false)
    {
        Value = value;
        _displayName = displayName;
        IsAllOption = isAllOption;
        _isSelected = isSelected;
    }

    public int Value { get; }

    public string DisplayName
    {
        get => _displayName;
        set
        {
            if (_displayName == value)
            {
                return;
            }

            _displayName = value;
            OnPropertyChanged();
        }
    }

    public bool IsAllOption { get; }

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected == value)
            {
                return;
            }

            _isSelected = value;
            OnPropertyChanged();
        }
    }
}