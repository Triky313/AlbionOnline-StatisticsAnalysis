using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.ViewModels;

namespace StatisticsAnalysisTool.Trade;

public sealed class TradeLocationFilterOption : BaseViewModel
{
    public TradeLocationFilterOption(MarketLocation location, string displayName, bool isAllOption, bool isSelected = false)
    {
        Location = location;
        DisplayName = displayName;
        IsAllOption = isAllOption;
        IsSelected = isSelected;
    }

    public MarketLocation Location { get; }

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