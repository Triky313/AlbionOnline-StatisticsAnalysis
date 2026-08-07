using StatisticsAnalysisTool.ViewModels;

namespace StatisticsAnalysisTool.Trade;

public sealed class TradeTimeRangeFilterOption : BaseViewModel
{
    public TradeTimeRangeFilterOption(TradeTimeRangePreset preset, string displayName)
    {
        Preset = preset;
        DisplayName = displayName;
    }

    public TradeTimeRangePreset Preset { get; }

    public string DisplayName { get; }

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