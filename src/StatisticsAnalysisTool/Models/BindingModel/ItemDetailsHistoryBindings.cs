using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.Models.ItemDetailsModel;
using StatisticsAnalysisTool.ViewModels;
using System.Collections.ObjectModel;

namespace StatisticsAnalysisTool.Models.BindingModel;

public class ItemDetailsHistoryBindings : BaseViewModel
{
    private readonly ItemDetailsViewModel _itemDetailsViewModel;
    private HistoryTimeRange _selectedTimeRange;
    private ObservableCollection<ISeries> _seriesHistory = new();
    private Axis[] _xAxesHistory;

    public ItemDetailsHistoryBindings(ItemDetailsViewModel itemDetailsViewModel)
    {
        _itemDetailsViewModel = itemDetailsViewModel;
        TimeRanges =
        [
            new HistoryTimeRange("7D", 7),
            new HistoryTimeRange("30D", 30),
            new HistoryTimeRange("90D", 90),
            new HistoryTimeRange("180D", 180),
            new HistoryTimeRange("1Y", 365),
            new HistoryTimeRange(LocalizationController.Translation("ALL"), 0)
        ];
        _selectedTimeRange = TimeRanges[1];
    }

    public ObservableCollection<HistoryTimeRange> TimeRanges { get; }

    public HistoryTimeRange SelectedTimeRange
    {
        get => _selectedTimeRange;
        set
        {
            if (ReferenceEquals(_selectedTimeRange, value))
            {
                return;
            }

            _selectedTimeRange = value;
            OnPropertyChanged();
            _itemDetailsViewModel.ApplyHistoryTimeRangeFilter();
        }
    }

    public ObservableCollection<ISeries> SeriesHistory
    {
        get => _seriesHistory;
        set
        {
            _seriesHistory = value;
            OnPropertyChanged();
        }
    }

    public Axis[] XAxesHistory
    {
        get => _xAxesHistory;
        set
        {
            _xAxesHistory = value;
            OnPropertyChanged();
        }
    }
}