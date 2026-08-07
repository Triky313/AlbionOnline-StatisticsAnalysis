using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace StatisticsAnalysisTool.Trade;

public class TradeMonitoringBindings : BaseViewModel
{
    private const int TargetVisibleProfitOverTimeLabels = 10;
    private const int ProfitByTimeOfDayHourCount = 24;
    private const int TopItemRankingLimit = 10;
    private readonly record struct TradeFilterContext(long FromTicks, long ToTicks, string SearchText, long? SearchNumber, int TierMask, int EnchantmentMask, IReadOnlySet<MarketLocation> Locations);
    private readonly record struct TradeFilterExecutionContext(List<Trade> TradesSnapshot, TradeFilterContext FilterContext);
    private readonly TradeProfitTimeSeriesService _tradeProfitTimeSeriesService = new();
    private readonly TradeProfitTimeOfDayService _tradeProfitTimeOfDayService = new();
    private readonly TradeItemRankingService _tradeItemRankingService = new();
    private readonly TradeLocationStatisticsService _tradeLocationStatisticsService = new();
    private IReadOnlyList<TradeProfitTimeSeriesPoint> _profitOverTimePoints = [];
    private IReadOnlyList<TradeProfitTimeOfDayPoint> _profitByTimeOfDayHourlyPoints = [];
    private TradeProfitTimeOfDayResult _profitByTimeOfDayResult = new();
    private int _profitOverTimeBucketStepSize = 1;

    public TradeMonitoringBindings()
    {
        TierFilters = BuildTierFilters();
        EnchantmentFilters = BuildEnchantmentFilters();
        LocationFilters = BuildLocationFilters();
        TimeRangeFilters = BuildTimeRangeFilters();
        ProfitOverTimeAggregationFilters = BuildProfitOverTimeAggregationFilters();
        ProfitByTimeOfDayChartModeFilters = BuildProfitByTimeOfDayChartModeFilters();
        ProfitByTimeOfDayMetricFilters = BuildProfitByTimeOfDayMetricFilters();
        Trades.CollectionChanged += UpdateTotalTradesUi;
        EnsureTradeCollectionViewInitialized();

        DatePickerTradeFrom = SettingsController.CurrentSettings.TradeMonitoringDatePickerTradeFrom;
        DatePickerTradeTo = DateTime.Today.AddDays(1);
        SynchronizeTimeRangeFilterSelection();
        UpdateProfitByTimeOfDayModeVisibility();
    }

    public void RefreshLocalization()
    {
        var selectedTierFilters = GetSelectedFilterValues(TierFilters);
        var selectedEnchantmentFilters = GetSelectedFilterValues(EnchantmentFilters);
        var selectedLocationFilters = GetSelectedLocationFilterValues(LocationFilters);
        var selectedTimeRangePreset = TimeRangeFilters.FirstOrDefault(option => option.IsSelected)?.Preset;
        var selectedProfitOverTimeAggregation = SelectedProfitOverTimeAggregation;
        var selectedProfitByTimeOfDayChartMode = SelectedProfitByTimeOfDayChartMode;
        var selectedProfitByTimeOfDayMetric = SelectedProfitByTimeOfDayMetric;

        TierFilters = BuildTierFilters();
        EnchantmentFilters = BuildEnchantmentFilters();
        LocationFilters = BuildLocationFilters();
        TimeRangeFilters = BuildTimeRangeFilters();
        ProfitOverTimeAggregationFilters = BuildProfitOverTimeAggregationFilters();
        ProfitByTimeOfDayChartModeFilters = BuildProfitByTimeOfDayChartModeFilters();
        ProfitByTimeOfDayMetricFilters = BuildProfitByTimeOfDayMetricFilters();

        RestoreFilterSelection(TierFilters, selectedTierFilters);
        RestoreFilterSelection(EnchantmentFilters, selectedEnchantmentFilters);
        RestoreLocationFilterSelection(LocationFilters, selectedLocationFilters);
        RestoreTimeRangeFilterSelection(selectedTimeRangePreset);
        SelectedProfitOverTimeAggregation = selectedProfitOverTimeAggregation;
        SelectedProfitByTimeOfDayChartMode = selectedProfitByTimeOfDayChartMode;
        SelectedProfitByTimeOfDayMetric = selectedProfitByTimeOfDayMetric;

        TradeStatsObject.RefreshLocalization();
        TradeOptionsObject.RefreshLocalization();
        ManuallyTradeMenuObject.RefreshLocalization();
        TradeExportTemplateObject.RefreshLocalization();

        ProfitOverTimeChartTitle = LocalizationController.Translation("PROFIT_OVER_TIME");
        ProfitByTimeOfDayChartTitle = LocalizationController.Translation("PROFIT_BY_TIME_OF_DAY");
        RefreshProfitOverTimeAxisLabels();
        RefreshProfitByTimeOfDayPresentation();
        _ = UpdateLocationStatisticsAsync();
    }

    public void ItemFilterReset()
    {
        ApplyTimeRangeFilter(TimeRangeFilters.First(option => option.Preset == TradeTimeRangePreset.All));
        TradesSearchText = string.Empty;
        SelectAllFilterOptions(TierFilters);
        SelectAllFilterOptions(EnchantmentFilters);
        SelectAllLocationFilterOptions(LocationFilters);

        EnsureTradeCollectionViewInitialized();

        if (TradeCollectionView == null)
        {
            return;
        }

        TradeCollectionView.Filter = null;
        var filteredTrades = TradeCollectionView.Cast<Trade>().ToList();
        TradeStatsObject?.SetTradeStats(filteredTrades);
        UpdateCurrentTradesUi(null, EventArgs.Empty);
        _ = UpdateStatisticViewsAsync(filteredTrades);
    }

    public ListCollectionView TradeCollectionView
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public ObservableRangeCollection<Trade> Trades
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = [];

    public string TradesSearchText
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<TradeNumericFilterOption> TierFilters
    {
        get;
        private set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<TradeNumericFilterOption> EnchantmentFilters
    {
        get;
        private set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<TradeLocationFilterOption> LocationFilters
    {
        get;
        private set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<TradeTimeRangeFilterOption> TimeRangeFilters
    {
        get;
        private set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public IReadOnlyList<KeyValuePair<TradeProfitTimeAggregation, string>> ProfitOverTimeAggregationFilters
    {
        get;
        private set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public IReadOnlyList<KeyValuePair<TradeTimeOfDayChartMode, string>> ProfitByTimeOfDayChartModeFilters
    {
        get;
        private set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public IReadOnlyList<KeyValuePair<TradeTimeOfDayMetric, string>> ProfitByTimeOfDayMetricFilters
    {
        get;
        private set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public TradeProfitTimeAggregation SelectedProfitOverTimeAggregation
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
    } = TradeProfitTimeAggregation.Day;

    public TradeProfitTimeAggregation EffectiveProfitOverTimeAggregation
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = TradeProfitTimeAggregation.Day;

    public TradeTimeOfDayChartMode SelectedProfitByTimeOfDayChartMode
    {
        get;
        set
        {
            if (field == value)
            {
                return;
            }

            field = value;
            UpdateProfitByTimeOfDayModeVisibility();
            OnPropertyChanged();
        }
    } = TradeTimeOfDayChartMode.Heatmap;

    public TradeTimeOfDayMetric SelectedProfitByTimeOfDayMetric
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
    } = TradeTimeOfDayMetric.NetProfit;

    public ObservableCollection<ISeries> ProfitOverTimeSeries
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = [];

    public ObservableCollection<ISeries> ProfitByTimeOfDaySeries
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = [];

    public ObservableCollection<TradeItemRankingEntry> TopItemsByProfit
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = [];

    public ObservableCollection<TradeItemRankingEntry> TopItemsByLoss
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = [];

    public ObservableCollection<TradeItemRankingEntry> TopItemsByRoi
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = [];

    public ObservableCollection<TradeItemRankingEntry> TopSoldItemsByVolume
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = [];

    public ObservableCollection<TradeItemRankingEntry> TopBoughtItemsByVolume
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = [];

    public ObservableCollection<TradeLocationStatisticsEntry> LocationStatistics
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = [];

    public Axis[] ProfitOverTimeXAxes
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = [];

    public Axis[] ProfitByTimeOfDayXAxes
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = [];

    public Axis[] ProfitByTimeOfDayYAxes
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } =
    [
        new Axis
        {
            LabelsRotation = 0
        }
    ];

    public Axis[] ProfitOverTimeYAxes
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } =
    [
        new Axis
        {
            LabelsRotation = 0,
            Labeler = value => value.ToShortNumberString()
        }
    ];

    public string ProfitOverTimeChartTitle
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = LocalizationController.Translation("PROFIT_OVER_TIME");

    public string ProfitByTimeOfDayChartTitle
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = LocalizationController.Translation("PROFIT_BY_TIME_OF_DAY");

    public IReadOnlyList<TradeProfitTimeOfDayPoint> ProfitByTimeOfDayHeatmapPoints
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = [];

    public Visibility ProfitByTimeOfDayHeatmapVisibility
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = Visibility.Visible;

    public Visibility ProfitByTimeOfDayBarChartVisibility
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = Visibility.Collapsed;

    public DateTime DatePickerTradeFrom
    {
        get;
        set
        {
            if (field == value)
            {
                return;
            }

            field = value;
            SettingsController.CurrentSettings.TradeMonitoringDatePickerTradeFrom = field;
            OnPropertyChanged();
        }
    } = new(2017, 1, 1);

    public DateTime DatePickerTradeTo
    {
        get;
        set
        {
            if (field == value)
            {
                return;
            }

            field = value;
            SettingsController.CurrentSettings.TradeMonitoringDatePickerTradeTo = field;
            OnPropertyChanged();
        }
    } = DateTime.Today.AddDays(1);

    public bool IsDeleteTradesButtonEnabled
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = true;

    public TradeStatsObject TradeStatsObject
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = new();

    public ManuallyTradeMenuObject ManuallyTradeMenuObject
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = new();

    public TradeExportTemplateObject TradeExportTemplateObject
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = new();

    public TradeOptionsObject TradeOptionsObject
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = new();

    public int TotalTradeCounts
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public int CurrentTradeCounts
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public Visibility IsTradeMonitoringPopupVisible
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = Visibility.Collapsed;

    public GridLength GridSplitterPosition
    {
        get;
        set
        {
            field = value;
            SettingsController.CurrentSettings.MailMonitoringGridSplitterPosition = field.Value;
            OnPropertyChanged();
        }
    } = GridLength.Auto;

    public Visibility FilteringIsRunningIconVisibility
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = Visibility.Collapsed;

    #region Update ui

    public void UpdateTotalTradesUi(object sender, NotifyCollectionChangedEventArgs e)
    {
        Application.Current.Dispatcher.InvokeAsync(() =>
        {
            TotalTradeCounts = Trades.Count;

            var allTimeRangeOption = TimeRangeFilters.First(option => option.Preset == TradeTimeRangePreset.All);
            if (allTimeRangeOption.IsSelected)
            {
                ApplyTimeRangeFilter(allTimeRangeOption);
            }
        });
    }

    public void UpdateCurrentTradesUi(object sender, EventArgs e)
    {
        Application.Current.Dispatcher.InvokeAsync(() =>
        {
            CurrentTradeCounts = TradeCollectionView?.Count ?? 0;
        });
    }

    #endregion

    #region Filter

    private CancellationTokenSource _cancellationTokenSource;

    public void UpdateTierFilterSelection(TradeNumericFilterOption selectedOption, bool isSelected)
    {
        UpdateFilterSelection(TierFilters, selectedOption, isSelected);
    }

    public void UpdateEnchantmentFilterSelection(TradeNumericFilterOption selectedOption, bool isSelected)
    {
        UpdateFilterSelection(EnchantmentFilters, selectedOption, isSelected);
    }

    public void UpdateLocationFilterSelection(TradeLocationFilterOption selectedOption, bool isSelected)
    {
        UpdateLocationFilterSelection(LocationFilters, selectedOption, isSelected);
    }

    public void ApplyTimeRangeFilter(TradeTimeRangeFilterOption selectedOption)
    {
        if (selectedOption == null || !TimeRangeFilters.Contains(selectedOption))
        {
            return;
        }

        var timeRange = GetTimeRange(selectedOption.Preset);
        DatePickerTradeFrom = timeRange.From;
        DatePickerTradeTo = timeRange.To;

        foreach (var option in TimeRangeFilters)
        {
            option.IsSelected = ReferenceEquals(option, selectedOption);
        }
    }

    public void SynchronizeTimeRangeFilterSelection()
    {
        foreach (var option in TimeRangeFilters)
        {
            var timeRange = GetTimeRange(option.Preset);
            var isMatch = DatePickerTradeFrom.Date == timeRange.From && DatePickerTradeTo.Date == timeRange.To;
            option.IsSelected = isMatch;
        }
    }

    public async Task UpdateFilteredTradesAsync()
    {
        CancellationTokenSource previousCancellationTokenSource = null;
        CancellationTokenSource currentCancellationTokenSource = null;
        CancellationToken currentCancellationToken = CancellationToken.None;
        TradeFilterExecutionContext? executionContext = null;

        await RunOnUiThreadAsync(() =>
        {
            if (Trades == null)
            {
                return;
            }

            FilteringIsRunningIconVisibility = Visibility.Visible;
            previousCancellationTokenSource = _cancellationTokenSource;
            currentCancellationTokenSource = new CancellationTokenSource();
            currentCancellationToken = currentCancellationTokenSource.Token;
            _cancellationTokenSource = currentCancellationTokenSource;
            executionContext = new TradeFilterExecutionContext(Trades.ToList(), BuildFilterContext());
        });

        if (executionContext == null || currentCancellationTokenSource == null)
        {
            return;
        }

        if (previousCancellationTokenSource is not null)
        {
            try
            {
                await previousCancellationTokenSource.CancelAsync();
            }
            catch (ObjectDisposedException)
            {
                // Ignored
            }
        }

        try
        {
            var filteredTrades = executionContext.Value.TradesSnapshot.Count <= 0
                ? []
                : await Task.Run(() => ParallelTradeFilterProcess(
                    executionContext.Value.TradesSnapshot,
                    executionContext.Value.FilterContext,
                    currentCancellationToken), CancellationToken.None);

            if (currentCancellationToken.IsCancellationRequested)
            {
                return;
            }

            await RunOnUiThreadAsync(() =>
            {
                EnsureTradeCollectionViewInitialized();

                if (TradeCollectionView == null)
                {
                    return;
                }

                var filterContext = executionContext.Value.FilterContext;
                TradeCollectionView.Filter = obj => Filter(obj, filterContext);
                TradeStatsObject?.SetTradeStats(TradeCollectionView.Cast<Trade>().ToList());
                CurrentTradeCounts = TradeCollectionView.Count;
            });

            if (currentCancellationToken.IsCancellationRequested)
            {
                return;
            }

            await UpdateStatisticViewsAsync(filteredTrades);
        }
        catch (OperationCanceledException) when (currentCancellationToken.IsCancellationRequested)
        {
            // Ignored
        }
        finally
        {
            await RunOnUiThreadAsync(() =>
            {
                if (!ReferenceEquals(_cancellationTokenSource, currentCancellationTokenSource))
                {
                    return;
                }

                FilteringIsRunningIconVisibility = Visibility.Collapsed;
                _cancellationTokenSource = null;
            });

            currentCancellationTokenSource.Dispose();
        }
    }

    public void EnsureTradeCollectionViewInitialized()
    {
        ConfigureTradeCollectionView(CollectionViewSource.GetDefaultView(Trades) as ListCollectionView);
    }

    private void ConfigureTradeCollectionView(ListCollectionView tradeCollectionView)
    {
        if (ReferenceEquals(TradeCollectionView, tradeCollectionView))
        {
            return;
        }

        if (TradeCollectionView != null)
        {
            TradeCollectionView.CurrentChanged -= UpdateCurrentTradesUi;
        }

        TradeCollectionView = tradeCollectionView;

        if (TradeCollectionView == null)
        {
            return;
        }

        TradeCollectionView.CurrentChanged += UpdateCurrentTradesUi;
        TradeCollectionView.IsLiveSorting = true;
        TradeCollectionView.IsLiveFiltering = true;
        TradeCollectionView.CustomSort = new TradeComparer();
        TradeCollectionView.Refresh();
    }

    private static List<Trade> ParallelTradeFilterProcess(IEnumerable<Trade> trades, TradeFilterContext context, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return [];
        }

        var partitioner = Partitioner.Create(trades, EnumerablePartitionerOptions.NoBuffering);
        var result = new ConcurrentBag<Trade>();

        Parallel.ForEach(partitioner, (trade, state) =>
        {
            if (cancellationToken.IsCancellationRequested)
            {
                state.Stop();
                return;
            }

            if (Filter(trade, context))
            {
                result.Add(trade);
            }
        });

        if (cancellationToken.IsCancellationRequested)
        {
            return [];
        }

        return result.OrderByDescending(d => d.Ticks).ToList();
    }

    public async Task UpdateProfitOverTimeChartAsync(IEnumerable<Trade> filteredTrades = null)
    {
        var tradeSnapshot = filteredTrades?.ToList() ?? await GetFilteredTradeSnapshotAsync();
        var chartResult = await Task.Run(() =>
        {
            return _tradeProfitTimeSeriesService.BuildTimeSeries(
                tradeSnapshot,
                DatePickerTradeFrom.Date,
                DatePickerTradeTo.Date,
                SelectedProfitOverTimeAggregation);
        });

        await Application.Current.Dispatcher.InvokeAsync(() =>
        {
            ApplyProfitOverTimeChart(chartResult);
        });
    }

    public async Task UpdateProfitByTimeOfDayChartAsync(IEnumerable<Trade> filteredTrades = null)
    {
        var tradeSnapshot = filteredTrades?.ToList() ?? await GetFilteredTradeSnapshotAsync();
        var chartResult = await Task.Run(() => _tradeProfitTimeOfDayService.Build(tradeSnapshot));

        await Application.Current.Dispatcher.InvokeAsync(() =>
        {
            ApplyProfitByTimeOfDayChart(chartResult);
        });
    }

    public async Task UpdateTopItemRankingsAsync(IEnumerable<Trade> filteredTrades = null)
    {
        var tradeSnapshot = filteredTrades?.ToList() ?? await GetFilteredTradeSnapshotAsync();
        var rankingResult = await Task.Run(() => _tradeItemRankingService.BuildRankings(tradeSnapshot, TopItemRankingLimit));

        await Application.Current.Dispatcher.InvokeAsync(() =>
        {
            TopItemsByProfit = [.. rankingResult.TopItemsByProfit];
            TopItemsByLoss = [.. rankingResult.TopItemsByLoss];
            TopItemsByRoi = [.. rankingResult.TopItemsByRoi];
            TopSoldItemsByVolume = [.. rankingResult.TopSoldItemsByVolume];
            TopBoughtItemsByVolume = [.. rankingResult.TopBoughtItemsByVolume];
        });
    }

    public async Task UpdateLocationStatisticsAsync(IEnumerable<Trade> filteredTrades = null)
    {
        var tradeSnapshot = filteredTrades?.ToList() ?? await GetFilteredTradeSnapshotAsync();
        var selectedLocations = GetSelectedLocationFilterValues(LocationFilters);
        var statistics = await Task.Run(() => _tradeLocationStatisticsService.Build(tradeSnapshot, selectedLocations));

        await Application.Current.Dispatcher.InvokeAsync(() =>
        {
            LocationStatistics = [.. statistics];
        });
    }

    private async Task UpdateStatisticViewsAsync(IEnumerable<Trade> filteredTrades = null)
    {
        var tradeSnapshot = filteredTrades?.ToList() ?? await GetFilteredTradeSnapshotAsync();

        await Task.WhenAll(
            UpdateProfitOverTimeChartAsync(tradeSnapshot),
            UpdateProfitByTimeOfDayChartAsync(tradeSnapshot),
            UpdateTopItemRankingsAsync(tradeSnapshot),
            UpdateLocationStatisticsAsync(tradeSnapshot));
    }

    private TradeFilterContext BuildFilterContext()
    {
        var searchText = TradesSearchText?.Trim() ?? string.Empty;
        var hasNumericSearch = long.TryParse(searchText, NumberStyles.Any, CultureInfo.CurrentCulture, out var searchNumber);
        var toDate = DatePickerTradeTo.Date;
        var toTicks = toDate == DateTime.MaxValue.Date ? DateTime.MaxValue.Ticks : toDate.AddDays(1).AddTicks(-1).Ticks;

        return new TradeFilterContext(
            DatePickerTradeFrom.Date.Ticks,
            toTicks,
            searchText,
            hasNumericSearch ? searchNumber : null,
            BuildSelectionMask(TierFilters),
            BuildSelectionMask(EnchantmentFilters),
            GetSelectedLocationFilterValues(LocationFilters));
    }

    private static bool Filter(object obj, TradeFilterContext context)
    {
        if (obj is not Trade trade)
        {
            return false;
        }

        if (trade.Ticks < context.FromTicks || trade.Ticks > context.ToTicks)
        {
            return false;
        }

        if (!MatchesTierFilter(trade, context.TierMask))
        {
            return false;
        }

        if (!MatchesLevelFilter(trade, context.EnchantmentMask))
        {
            return false;
        }

        if (!MatchesLocationFilter(trade, context.Locations))
        {
            return false;
        }

        if (string.IsNullOrEmpty(context.SearchText))
        {
            return true;
        }

        if (context.SearchNumber is { } searchNumber)
        {
            return trade.MailContent?.UnitPriceWithoutTax.IntegerValue == searchNumber ||
                   trade.MailContent?.TotalPrice.IntegerValue == searchNumber ||
                   trade.InstantBuySellContent?.UnitPrice.IntegerValue == searchNumber ||
                   trade.InstantBuySellContent?.TotalPrice.IntegerValue == searchNumber ||
                   trade.PlayerTradeContent?.Silver.IntegerValue == searchNumber ||
                   trade.PlayerTradeContent?.Quantity == searchNumber;
        }

        return (trade.LocationName?.IndexOf(context.SearchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
               ($"T{trade.Item?.Tier}.{trade.Item?.Level}".IndexOf(context.SearchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
               (trade.MailTypeDescription?.IndexOf(context.SearchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
               (trade.Item?.LocalizedName?.IndexOf(context.SearchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
               (trade.MailContent?.UnitPriceWithoutTax.ToString().IndexOf(context.SearchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
               (trade.MailContent?.TotalPrice.ToString().IndexOf(context.SearchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
               (trade.InstantBuySellContent?.UnitPrice.ToString().IndexOf(context.SearchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
               (trade.InstantBuySellContent?.TotalPrice.ToString().IndexOf(context.SearchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
               (trade.PlayerTradeContent?.PartnerName?.IndexOf(context.SearchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
               (trade.PlayerTradeContent?.Silver.ToString().IndexOf(context.SearchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
               (trade.Description?.IndexOf(context.SearchText, StringComparison.OrdinalIgnoreCase) >= 0);
    }

    private static ObservableCollection<TradeNumericFilterOption> BuildTierFilters()
    {
        return
        [
            new(0, LocalizationController.Translation("ALL"), true, true),
            new(1, "T1", false),
            new(2, "T2", false),
            new(3, "T3", false),
            new(4, "T4", false),
            new(5, "T5", false),
            new(6, "T6", false),
            new(7, "T7", false),
            new(8, "T8", false)
        ];
    }

    private static ObservableCollection<TradeNumericFilterOption> BuildEnchantmentFilters()
    {
        return
        [
            new(-1, LocalizationController.Translation("ALL"), true, true),
            new(0, "0", false),
            new(1, "1", false),
            new(2, "2", false),
            new(3, "3", false),
            new(4, "4", false)
        ];
    }

    private static HashSet<int> GetSelectedFilterValues(IEnumerable<TradeNumericFilterOption> options)
    {
        return options
            .Where(option => !option.IsAllOption && option.IsSelected)
            .Select(option => option.Value)
            .ToHashSet();
    }

    private static void RestoreFilterSelection(IReadOnlyList<TradeNumericFilterOption> options, IReadOnlySet<int> selectedValues)
    {
        var hasSpecificSelection = options.Any(option => !option.IsAllOption && selectedValues.Contains(option.Value));

        foreach (var option in options)
        {
            option.IsSelected = option.IsAllOption
                ? !hasSpecificSelection
                : selectedValues.Contains(option.Value);
        }
    }

    private static void SelectAllFilterOptions(IEnumerable<TradeNumericFilterOption> options)
    {
        foreach (var option in options)
        {
            option.IsSelected = option.IsAllOption;
        }
    }

    private static void UpdateFilterSelection(
        IReadOnlyList<TradeNumericFilterOption> options,
        TradeNumericFilterOption selectedOption,
        bool isSelected)
    {
        if (selectedOption == null || !options.Contains(selectedOption))
        {
            return;
        }

        selectedOption.IsSelected = isSelected;
        var allOption = options.First(option => option.IsAllOption);

        if (selectedOption.IsAllOption)
        {
            if (isSelected)
            {
                foreach (var option in options.Where(option => !option.IsAllOption))
                {
                    option.IsSelected = false;
                }
            }
            else if (!options.Any(option => !option.IsAllOption && option.IsSelected))
            {
                allOption.IsSelected = true;
            }

            return;
        }

        if (isSelected)
        {
            allOption.IsSelected = false;
        }
        else if (!options.Any(option => !option.IsAllOption && option.IsSelected))
        {
            allOption.IsSelected = true;
        }
    }

    private static int BuildSelectionMask(IEnumerable<TradeNumericFilterOption> options)
    {
        return options
            .Where(option => !option.IsAllOption && option.IsSelected)
            .Aggregate(0, (mask, option) => mask | (1 << option.Value));
    }

    private static ObservableCollection<TradeLocationFilterOption> BuildLocationFilters()
    {
        var filters = new ObservableCollection<TradeLocationFilterOption>
        {
            new(MarketLocation.Unknown, LocalizationController.Translation("ALL"), true, true)
        };

        foreach (var location in TradeLocationStatisticsService.SupportedMarketLocations)
        {
            filters.Add(new TradeLocationFilterOption(location.Key, location.Value, false));
        }

        return filters;
    }

    private static ObservableCollection<TradeTimeRangeFilterOption> BuildTimeRangeFilters()
    {
        var days = LocalizationController.Translation("DAYS");
        var year = LocalizationController.Translation("YEAR");

        return
        [
            new(TradeTimeRangePreset.All, LocalizationController.Translation("ALL")),
            new(TradeTimeRangePreset.Today, LocalizationController.Translation("TODAY")),
            new(TradeTimeRangePreset.Last7Days, $"7 {days}"),
            new(TradeTimeRangePreset.Last30Days, $"30 {days}"),
            new(TradeTimeRangePreset.Last90Days, $"90 {days}"),
            new(TradeTimeRangePreset.LastYear, $"1 {year}")
        ];
    }

    private static HashSet<MarketLocation> GetSelectedLocationFilterValues(IEnumerable<TradeLocationFilterOption> options)
    {
        return options
            .Where(option => !option.IsAllOption && option.IsSelected)
            .Select(option => option.Location)
            .ToHashSet();
    }

    private static void RestoreLocationFilterSelection(
        IReadOnlyList<TradeLocationFilterOption> options,
        IReadOnlySet<MarketLocation> selectedLocations)
    {
        var hasSpecificSelection = options.Any(option => !option.IsAllOption && selectedLocations.Contains(option.Location));

        foreach (var option in options)
        {
            option.IsSelected = option.IsAllOption
                ? !hasSpecificSelection
                : selectedLocations.Contains(option.Location);
        }
    }

    private static void SelectAllLocationFilterOptions(IEnumerable<TradeLocationFilterOption> options)
    {
        foreach (var option in options)
        {
            option.IsSelected = option.IsAllOption;
        }
    }

    private static void UpdateLocationFilterSelection(
        IReadOnlyList<TradeLocationFilterOption> options,
        TradeLocationFilterOption selectedOption,
        bool isSelected)
    {
        if (selectedOption == null || !options.Contains(selectedOption))
        {
            return;
        }

        selectedOption.IsSelected = isSelected;
        var allOption = options.First(option => option.IsAllOption);

        if (selectedOption.IsAllOption)
        {
            if (isSelected)
            {
                foreach (var option in options.Where(option => !option.IsAllOption))
                {
                    option.IsSelected = false;
                }
            }
            else if (!options.Any(option => !option.IsAllOption && option.IsSelected))
            {
                allOption.IsSelected = true;
            }

            return;
        }

        if (isSelected)
        {
            allOption.IsSelected = false;
        }
        else if (!options.Any(option => !option.IsAllOption && option.IsSelected))
        {
            allOption.IsSelected = true;
        }
    }

    private void RestoreTimeRangeFilterSelection(TradeTimeRangePreset? selectedPreset)
    {
        foreach (var option in TimeRangeFilters)
        {
            option.IsSelected = selectedPreset.HasValue && option.Preset == selectedPreset.Value;
        }
    }

    private (DateTime From, DateTime To) GetTimeRange(TradeTimeRangePreset preset)
    {
        var today = DateTime.Today;

        return preset switch
        {
            TradeTimeRangePreset.All => (GetOldestTradeDate(), today.AddDays(1)),
            TradeTimeRangePreset.Today => (today, today),
            TradeTimeRangePreset.Last7Days => (today.AddDays(-7), today),
            TradeTimeRangePreset.Last30Days => (today.AddDays(-30), today),
            TradeTimeRangePreset.Last90Days => (today.AddDays(-90), today),
            TradeTimeRangePreset.LastYear => (today.AddYears(-1), today),
            _ => (today, today)
        };
    }

    private DateTime GetOldestTradeDate()
    {
        return Trades
            .Where(trade => trade != null && trade.Ticks > 0 && trade.Ticks <= DateTime.MaxValue.Ticks)
            .Select(trade => new DateTime(trade.Ticks).Date)
            .DefaultIfEmpty(new DateTime(2017, 1, 1))
            .Min();
    }

    private static IReadOnlyList<KeyValuePair<TradeProfitTimeAggregation, string>> BuildProfitOverTimeAggregationFilters()
    {
        return new List<KeyValuePair<TradeProfitTimeAggregation, string>>
        {
            new(TradeProfitTimeAggregation.Hour, LocalizationController.Translation("HOUR")),
            new(TradeProfitTimeAggregation.Day, LocalizationController.Translation("DAY")),
            new(TradeProfitTimeAggregation.Week, LocalizationController.Translation("WEEK")),
            new(TradeProfitTimeAggregation.Month, LocalizationController.Translation("MONTH")),
            new(TradeProfitTimeAggregation.Year, LocalizationController.Translation("YEAR"))
        };
    }

    private static IReadOnlyList<KeyValuePair<TradeTimeOfDayChartMode, string>> BuildProfitByTimeOfDayChartModeFilters()
    {
        return new List<KeyValuePair<TradeTimeOfDayChartMode, string>>
        {
            new(TradeTimeOfDayChartMode.Heatmap, LocalizationController.Translation("HEATMAP")),
            new(TradeTimeOfDayChartMode.HourlyBars, LocalizationController.Translation("HOURLY_BARS"))
        };
    }

    private static IReadOnlyList<KeyValuePair<TradeTimeOfDayMetric, string>> BuildProfitByTimeOfDayMetricFilters()
    {
        return new List<KeyValuePair<TradeTimeOfDayMetric, string>>
        {
            new(TradeTimeOfDayMetric.NetProfit, LocalizationController.Translation("NET_PROFIT")),
            new(TradeTimeOfDayMetric.AverageProfitPerTrade, LocalizationController.Translation("AVERAGE_PROFIT_PER_TRADE")),
            new(TradeTimeOfDayMetric.TradeCount, LocalizationController.Translation("TRADE_COUNT"))
        };
    }

    private static bool MatchesTierFilter(Trade trade, int tierMask)
    {
        if (tierMask == 0)
        {
            return true;
        }

        var itemTier = trade.Item?.Tier ?? 0;
        return itemTier is >= 1 and <= 8 && (tierMask & (1 << itemTier)) != 0;
    }

    private static bool MatchesLevelFilter(Trade trade, int enchantmentMask)
    {
        if (enchantmentMask == 0)
        {
            return true;
        }

        var itemLevel = trade.Item?.Level;
        return itemLevel is >= 0 and <= 4 && (enchantmentMask & (1 << itemLevel.Value)) != 0;
    }

    private static bool MatchesLocationFilter(Trade trade, IReadOnlySet<MarketLocation> selectedLocations)
    {
        if (selectedLocations.Count == 0)
        {
            return true;
        }

        return selectedLocations.Contains(trade.Location);
    }

    private Task<List<Trade>> GetFilteredTradeSnapshotAsync()
    {
        return RunOnUiThreadAsync(() =>
        {
            if (TradeCollectionView == null)
            {
                return Trades?.ToList() ?? [];
            }

            return TradeCollectionView.Cast<Trade>().ToList();
        });
    }

    private void ApplyProfitOverTimeChart(TradeProfitTimeSeriesResult chartResult)
    {
        _profitOverTimePoints = chartResult.Points ?? [];
        _profitOverTimeBucketStepSize = chartResult.BucketStepSize;
        EffectiveProfitOverTimeAggregation = chartResult.EffectiveAggregation;
        ProfitOverTimeChartTitle = LocalizationController.Translation("PROFIT_OVER_TIME");

        ProfitOverTimeXAxes =
        [
            new Axis
            {
                LabelsRotation = 15,
                Labels = BuildProfitOverTimeLabels(_profitOverTimePoints, chartResult.EffectiveAggregation, chartResult.BucketStepSize)
            }
        ];

        if (_profitOverTimePoints.Count == 0)
        {
            ProfitOverTimeSeries = [];
            return;
        }

        ProfitOverTimeSeries =
        [
            CreateProfitOverTimeSeries(isPositiveSeries: true, "SolidColorBrush.Accent.Blue.3"),
            CreateProfitOverTimeSeries(isPositiveSeries: false, "SolidColorBrush.Accent.Red.4")
        ];
    }

    private void RefreshProfitOverTimeAxisLabels()
    {
        ProfitOverTimeXAxes =
        [
            new Axis
            {
                LabelsRotation = 15,
                Labels = BuildProfitOverTimeLabels(_profitOverTimePoints, EffectiveProfitOverTimeAggregation, _profitOverTimeBucketStepSize)
            }
        ];
    }

    public void RefreshProfitByTimeOfDayPresentation()
    {
        ApplyProfitByTimeOfDayChart(_profitByTimeOfDayResult);
    }

    public TradeProfitTimeOfDayPoint GetProfitByTimeOfDayHeatmapPoint(DayOfWeek dayOfWeek, int hour)
    {
        return ProfitByTimeOfDayHeatmapPoints.FirstOrDefault(x => x.DayOfWeek == dayOfWeek && x.Hour == hour);
    }

    public string FormatProfitByTimeOfDayTooltip(TradeProfitTimeOfDayPoint point)
    {
        if (point == null)
        {
            return string.Empty;
        }

        var lineBreak = Environment.NewLine;
        var periodLabel = $"{point.Hour:00}:00 - {((point.Hour + 1) % ProfitByTimeOfDayHourCount):00}:00";
        var dayLabel = point.DayOfWeek.HasValue
            ? $"{CultureInfo.CurrentCulture.DateTimeFormat.GetDayName(point.DayOfWeek.Value)}{lineBreak}"
            : string.Empty;

        return
            $"{dayLabel}{periodLabel}{lineBreak}" +
            $"{LocalizationController.Translation("NET_PROFIT")}: {point.NetProfit.ToChartTooltipNumberString()}{lineBreak}" +
            $"{LocalizationController.Translation("AVERAGE_PROFIT_PER_TRADE")}: {point.AverageNetProfitPerTrade.ToChartTooltipNumberString()}{lineBreak}" +
            $"{LocalizationController.Translation("TRADE_COUNT")}: {point.TradeCount.ToString("N0", CultureInfo.CurrentCulture)}{lineBreak}" +
            $"{LocalizationController.Translation("SOLD")}: {point.Sold.ToChartTooltipNumberString()}{lineBreak}" +
            $"{LocalizationController.Translation("BOUGHT")}: {point.Bought.ToChartTooltipNumberString()}{lineBreak}" +
            $"{LocalizationController.Translation("TAX")}: {point.Tax.ToChartTooltipNumberString()}";
    }

    private void ApplyProfitByTimeOfDayChart(TradeProfitTimeOfDayResult chartResult)
    {
        _profitByTimeOfDayResult = chartResult ?? new TradeProfitTimeOfDayResult();
        ProfitByTimeOfDayHeatmapPoints = _profitByTimeOfDayResult.HeatmapPoints ?? [];
        _profitByTimeOfDayHourlyPoints = _profitByTimeOfDayResult.HourlyPoints ?? [];
        ProfitByTimeOfDayChartTitle = LocalizationController.Translation("PROFIT_BY_TIME_OF_DAY");

        ProfitByTimeOfDayXAxes =
        [
            new Axis
            {
                LabelsRotation = 0,
                Labels = Enumerable.Range(0, ProfitByTimeOfDayHourCount)
                    .Select(hour => hour.ToString("00", CultureInfo.CurrentCulture))
                    .ToArray()
            }
        ];

        ProfitByTimeOfDayYAxes =
        [
            new Axis
            {
                LabelsRotation = 0,
                Labeler = BuildProfitByTimeOfDayAxisLabeler()
            }
        ];

        UpdateProfitByTimeOfDayModeVisibility();
        ProfitByTimeOfDaySeries = BuildProfitByTimeOfDaySeries();
    }

    private ObservableCollection<ISeries> BuildProfitByTimeOfDaySeries()
    {
        if (_profitByTimeOfDayHourlyPoints.Count == 0)
        {
            return new ObservableCollection<ISeries>();
        }

        var metricValues = _profitByTimeOfDayHourlyPoints
            .Select(GetProfitByTimeOfDayMetricValue)
            .ToList();

        var hasNegativeValues = metricValues.Any(value => value < 0d);
        if (!hasNegativeValues)
        {
            return new ObservableCollection<ISeries>
            {
                CreateProfitByTimeOfDayBarSeries(isPositiveSeries: true, "SolidColorBrush.Accent.Green.3")
            };
        }

        return new ObservableCollection<ISeries>
        {
            CreateProfitByTimeOfDayBarSeries(isPositiveSeries: true, "SolidColorBrush.Accent.Green.3"),
            CreateProfitByTimeOfDayBarSeries(isPositiveSeries: false, "SolidColorBrush.Accent.Red.3")
        };
    }

    private ISeries CreateProfitByTimeOfDayBarSeries(bool isPositiveSeries, string resourceKey)
    {
        var values = new ObservableCollection<double>();

        for (var index = 0; index < _profitByTimeOfDayHourlyPoints.Count; index++)
        {
            var value = GetProfitByTimeOfDayMetricValue(_profitByTimeOfDayHourlyPoints[index]);
            values.Add(isPositiveSeries ? Math.Max(0d, value) : Math.Min(0d, value));
        }

        return new ColumnSeries<double>
        {
            Name = string.Empty,
            Values = values,
            Stroke = null,
            Fill = CreatePaint(resourceKey),
            MaxBarWidth = 18,
            YToolTipLabelFormatter = chartPoint =>
            {
                var index = (int) Math.Round(chartPoint.Coordinate.SecondaryValue);
                return index >= 0 && index < _profitByTimeOfDayHourlyPoints.Count
                    ? FormatProfitByTimeOfDayTooltip(_profitByTimeOfDayHourlyPoints[index])
                    : string.Empty;
            }
        };
    }

    private Func<double, string> BuildProfitByTimeOfDayAxisLabeler()
    {
        return SelectedProfitByTimeOfDayMetric == TradeTimeOfDayMetric.TradeCount
            ? value => Math.Round(value).ToString("N0", CultureInfo.CurrentCulture)
            : value => value.ToShortNumberString();
    }

    private double GetProfitByTimeOfDayMetricValue(TradeProfitTimeOfDayPoint point)
    {
        return point?.GetMetricValue(SelectedProfitByTimeOfDayMetric) ?? 0d;
    }

    private void UpdateProfitByTimeOfDayModeVisibility()
    {
        var isHeatmapVisible = SelectedProfitByTimeOfDayChartMode == TradeTimeOfDayChartMode.Heatmap;
        ProfitByTimeOfDayHeatmapVisibility = isHeatmapVisible ? Visibility.Visible : Visibility.Collapsed;
        ProfitByTimeOfDayBarChartVisibility = isHeatmapVisible ? Visibility.Collapsed : Visibility.Visible;
    }

    private ISeries CreateProfitOverTimeSeries(bool isPositiveSeries, string resourceKey)
    {
        var values = new ObservableCollection<double>();

        for (var i = 0; i < _profitOverTimePoints.Count; i++)
        {
            var point = _profitOverTimePoints[i];
            var value = point.NetProfit;
            values.Add(isPositiveSeries ? Math.Max(0d, value) : Math.Min(0d, value));
        }

        var fill = CreatePaint(resourceKey);

        return new ColumnSeries<double>
        {
            Name = string.Empty,
            Values = values,
            Stroke = null,
            Fill = fill,
            MaxBarWidth = 20,
            YToolTipLabelFormatter = chartPoint => chartPoint.Coordinate.PrimaryValue.ToChartTooltipNumberString()
        };
    }

    private static string[] BuildProfitOverTimeLabels(IReadOnlyList<TradeProfitTimeSeriesPoint> points, TradeProfitTimeAggregation aggregation, int bucketStepSize)
    {
        if (points == null || points.Count == 0)
        {
            return [];
        }

        var labelStep = Math.Max(1, (int) Math.Ceiling(points.Count / (double) TargetVisibleProfitOverTimeLabels));
        var labels = new string[points.Count];

        for (var i = 0; i < points.Count; i++)
        {
            labels[i] = i % labelStep == 0 || i == points.Count - 1 ? FormatAxisLabel(points[i], aggregation, bucketStepSize) : string.Empty;
        }

        return labels;
    }

    private static string FormatAxisLabel(TradeProfitTimeSeriesPoint point, TradeProfitTimeAggregation aggregation, int bucketStepSize)
    {
        return aggregation switch
        {
            TradeProfitTimeAggregation.Hour => point.PeriodStart.ToString("HH:mm", CultureInfo.CurrentCulture),
            TradeProfitTimeAggregation.Day => point.PeriodStart.ToString("HH:mm", CultureInfo.CurrentCulture),
            TradeProfitTimeAggregation.Week => point.PeriodStart.ToString("dd.MM", CultureInfo.CurrentCulture),
            TradeProfitTimeAggregation.Month => point.PeriodStart.ToString("dd.MM", CultureInfo.CurrentCulture),
            TradeProfitTimeAggregation.Year => point.PeriodStart.ToString("MM.yy", CultureInfo.CurrentCulture),
            _ => point.PeriodStart.ToString("g", CultureInfo.CurrentCulture)
        };
    }

    private static SolidColorPaint CreatePaint(string resourceKey)
    {
        if (Application.Current.Resources[resourceKey] is SolidColorBrush brush)
        {
            return new SolidColorPaint
            {
                Color = new SKColor(brush.Color.R, brush.Color.G, brush.Color.B, brush.Color.A)
            };
        }

        return new SolidColorPaint
        {
            Color = new SKColor(0, 0, 0, 0)
        };
    }

    #endregion
}
