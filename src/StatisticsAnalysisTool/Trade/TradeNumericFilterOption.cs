using StatisticsAnalysisTool.ViewModels;

namespace StatisticsAnalysisTool.Trade;

public sealed class TradeNumericFilterOption : BaseViewModel
{
    public TradeNumericFilterOption(int value, string displayName, bool isAllOption, bool isSelected = false)
    {
        Value = value;
        DisplayName = displayName;
        IsAllOption = isAllOption;
        IsSelected = isSelected;
    }

    public int Value { get; }

    public string DisplayName { get; }

    public bool IsAllOption { get; }

    public bool IsSelected
    {
        get;
        set
        {
            if (field == value)
            {
                return;
            }

            field = value;
            OnPropertyChanged();
        }
    }
}