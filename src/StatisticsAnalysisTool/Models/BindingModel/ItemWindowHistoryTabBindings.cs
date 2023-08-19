using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.ViewModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using static StatisticsAnalysisTool.Models.BindingModel.ItemWindowMainTabBindings;

namespace StatisticsAnalysisTool.Models.BindingModel;

public class ItemWindowHistoryTabBindings : BaseViewModel
{
    private readonly ItemWindowViewModel _itemWindowViewModel;
    private List<QualityStruct> _qualities = new();
    private QualityStruct _qualitiesSelection;
    private ObservableCollection<ISeries> _seriesHistory = new();
    private Axis[] _xAxesHistory;

    public ItemWindowHistoryTabBindings(ItemWindowViewModel itemWindowViewModel)
    {
        _itemWindowViewModel = itemWindowViewModel;
    }

    #region Bindings

    public List<QualityStruct> Qualities
    {
        get => _qualities;
        set
        {
            _qualities = value;
            OnPropertyChanged();
        }
    }

    public QualityStruct QualitiesSelection
    {
        get => _qualitiesSelection;
        set
        {
            _qualitiesSelection = value;
            SettingsController.CurrentSettings.ItemWindowHistoryTabQualitySelection = _qualitiesSelection.Quality;
            _itemWindowViewModel.UpdateHistoryTabChartPrices(null, null);
            OnPropertyChanged();
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

    #endregion
}