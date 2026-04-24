using LiveChartsCore;
using LiveChartsCore.Defaults;
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
using System.Collections.Specialized;
using System.Collections.ObjectModel;
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
    private readonly record struct TradeFilterContext(long FromTicks, long ToTicks, string SearchText, long? SearchNumber, int Tier, MarketLocation Location);

    private readonly TradeProfitTimeSeriesService _tradeProfitTimeSeriesService = new();
    private ListCollectionView _tradeCollectionView;
    private ObservableRangeCollection<Trade> _trades = new();
    private string _tradesSearchText;
    private DateTime _datePickerTradeFrom = new(2017, 1, 1);
    private DateTime _datePickerTradeTo = DateTime.UtcNow.AddDays(1);
    private TradeStatsObject _tradeStatsObject = new();
    private TradeOptionsObject _tradeOptionsObject = new();
    private Visibility _isTradeMonitoringPopupVisible = Visibility.Collapsed;
    private GridLength _gridSplitterPosition = GridLength.Auto;
    private int _totalTradeCounts;
    private int _currentTradeCounts;
    private ManuallyTradeMenuObject _tradeManuallyMenuObject = new();
    private bool _isDeleteTradesButtonEnabled = true;
    private Visibility _filteringIsRunningIconVisibility = Visibility.Collapsed;
    private TradeExportTemplateObject _tradeExportTemplateObject = new();
    private int _selectedTierFilter;
    private MarketLocation _selectedLocationFilter = MarketLocation.Unknown;
    private TradeProfitTimeAggregation _selectedProfitOverTimeAggregation = TradeProfitTimeAggregation.Day;
    private TradeProfitTimeAggregation _effectiveProfitOverTimeAggregation = TradeProfitTimeAggregation.Day;
    private ObservableCollection<ISeries> _profitOverTimeSeries = [];
    private Axis[] _profitOverTimeXAxes = [];
    private Axis[] _profitOverTimeYAxes =
    [
        new Axis
        {
            LabelsRotation = 0,
            Labeler = value => value.ToShortNumberString()
        }
    ];
    private IReadOnlyList<TradeProfitTimeSeriesPoint> _profitOverTimePoints = [];
    private string _profitOverTimeChartTitle = LocalizationController.Translation("PROFIT_OVER_TIME");

    public TradeMonitoringBindings()
    {
        TierFilters = BuildTierFilters();
        LocationFilters = BuildLocationFilters();
        ProfitOverTimeAggregationFilters = BuildProfitOverTimeAggregationFilters();
        TradeCollectionView = CollectionViewSource.GetDefaultView(Trades) as ListCollectionView;

        if (TradeCollectionView != null)
        {
            Trades.CollectionChanged += UpdateTotalTradesUi;
            TradeCollectionView.CurrentChanged += UpdateCurrentTradesUi;

            TradeCollectionView.IsLiveSorting = true;
            TradeCollectionView.IsLiveFiltering = true;
            TradeCollectionView.CustomSort = new TradeComparer();
            TradeCollectionView.Refresh();
        }

        DatePickerTradeFrom = SettingsController.CurrentSettings.TradeMonitoringDatePickerTradeFrom;
        DatePickerTradeTo = SettingsController.CurrentSettings.TradeMonitoringDatePickerTradeTo;
    }

    public void ItemFilterReset()
    {
        DatePickerTradeFrom = new DateTime(2017, 1, 1);
        DatePickerTradeTo = DateTime.UtcNow.AddDays(1);
        TradesSearchText = string.Empty;
        SelectedTierFilter = 0;
        SelectedLocationFilter = MarketLocation.Unknown;

        TradeCollectionView ??= CollectionViewSource.GetDefaultView(Trades) as ListCollectionView;

        if (TradeCollectionView == null)
        {
            return;
        }

        TradeCollectionView.Filter = null;
        var filteredTrades = TradeCollectionView.Cast<Trade>().ToList();
        TradeStatsObject?.SetTradeStats(filteredTrades);
        UpdateCurrentTradesUi(null, EventArgs.Empty);
        _ = UpdateProfitOverTimeChartAsync(filteredTrades);
    }

    public ListCollectionView TradeCollectionView
    {
        get => _tradeCollectionView;
        set
        {
            _tradeCollectionView = value;
            OnPropertyChanged();
        }
    }

    public ObservableRangeCollection<Trade> Trades
    {
        get => _trades;
        set
        {
            _trades = value;
            OnPropertyChanged();
        }
    }

    public string TradesSearchText
    {
        get => _tradesSearchText;
        set
        {
            _tradesSearchText = value;
            OnPropertyChanged();
        }
    }

    public IReadOnlyList<KeyValuePair<int, string>> TierFilters
    {
        get;
    }

    public int SelectedTierFilter
    {
        get => _selectedTierFilter;
        set
        {
            _selectedTierFilter = value;
            OnPropertyChanged();
        }
    }

    public IReadOnlyList<KeyValuePair<MarketLocation, string>> LocationFilters
    {
        get;
    }

    public MarketLocation SelectedLocationFilter
    {
        get => _selectedLocationFilter;
        set
        {
            _selectedLocationFilter = value;
            OnPropertyChanged();
        }
    }

    public IReadOnlyList<KeyValuePair<TradeProfitTimeAggregation, string>> ProfitOverTimeAggregationFilters
    {
        get;
    }

    public TradeProfitTimeAggregation SelectedProfitOverTimeAggregation
    {
        get => _selectedProfitOverTimeAggregation;
        set
        {
            if (_selectedProfitOverTimeAggregation == value)
            {
                return;
            }

            _selectedProfitOverTimeAggregation = value;
            OnPropertyChanged();
        }
    }

    public TradeProfitTimeAggregation EffectiveProfitOverTimeAggregation
    {
        get => _effectiveProfitOverTimeAggregation;
        set
        {
            _effectiveProfitOverTimeAggregation = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<ISeries> ProfitOverTimeSeries
    {
        get => _profitOverTimeSeries;
        set
        {
            _profitOverTimeSeries = value;
            OnPropertyChanged();
        }
    }

    public Axis[] ProfitOverTimeXAxes
    {
        get => _profitOverTimeXAxes;
        set
        {
            _profitOverTimeXAxes = value;
            OnPropertyChanged();
        }
    }

    public Axis[] ProfitOverTimeYAxes
    {
        get => _profitOverTimeYAxes;
        set
        {
            _profitOverTimeYAxes = value;
            OnPropertyChanged();
        }
    }

    public string ProfitOverTimeChartTitle
    {
        get => _profitOverTimeChartTitle;
        set
        {
            _profitOverTimeChartTitle = value;
            OnPropertyChanged();
        }
    }

    public DateTime DatePickerTradeFrom
    {
        get => _datePickerTradeFrom;
        set
        {
            _datePickerTradeFrom = value;
            SettingsController.CurrentSettings.TradeMonitoringDatePickerTradeFrom = _datePickerTradeFrom;
            OnPropertyChanged();
        }
    }

    public DateTime DatePickerTradeTo
    {
        get => _datePickerTradeTo;
        set
        {
            _datePickerTradeTo = value;
            SettingsController.CurrentSettings.TradeMonitoringDatePickerTradeTo = _datePickerTradeTo;
            OnPropertyChanged();
        }
    }

    public bool IsDeleteTradesButtonEnabled
    {
        get => _isDeleteTradesButtonEnabled;
        set
        {
            _isDeleteTradesButtonEnabled = value;
            OnPropertyChanged();
        }
    }

    public TradeStatsObject TradeStatsObject
    {
        get => _tradeStatsObject;
        set
        {
            _tradeStatsObject = value;
            OnPropertyChanged();
        }
    }

    public ManuallyTradeMenuObject ManuallyTradeMenuObject
    {
        get => _tradeManuallyMenuObject;
        set
        {
            _tradeManuallyMenuObject = value;
            OnPropertyChanged();
        }
    }

    public TradeExportTemplateObject TradeExportTemplateObject
    {
        get => _tradeExportTemplateObject;
        set
        {
            _tradeExportTemplateObject = value;
            OnPropertyChanged();
        }
    }

    public TradeOptionsObject TradeOptionsObject
    {
        get => _tradeOptionsObject;
        set
        {
            _tradeOptionsObject = value;
            OnPropertyChanged();
        }
    }

    public int TotalTradeCounts
    {
        get => _totalTradeCounts;
        set
        {
            _totalTradeCounts = value;
            OnPropertyChanged();
        }
    }

    public int CurrentTradeCounts
    {
        get => _currentTradeCounts;
        set
        {
            _currentTradeCounts = value;
            OnPropertyChanged();
        }
    }

    public Visibility IsTradeMonitoringPopupVisible
    {
        get => _isTradeMonitoringPopupVisible;
        set
        {
            _isTradeMonitoringPopupVisible = value;
            OnPropertyChanged();
        }
    }

    public GridLength GridSplitterPosition
    {
        get => _gridSplitterPosition;
        set
        {
            _gridSplitterPosition = value;
            SettingsController.CurrentSettings.MailMonitoringGridSplitterPosition = _gridSplitterPosition.Value;
            OnPropertyChanged();
        }
    }

    public Visibility FilteringIsRunningIconVisibility
    {
        get => _filteringIsRunningIconVisibility;
        set
        {
            _filteringIsRunningIconVisibility = value;
            OnPropertyChanged();
        }
    }

    #region Update ui

    public void UpdateTotalTradesUi(object sender, NotifyCollectionChangedEventArgs e)
    {
        Application.Current.Dispatcher.InvokeAsync(() =>
        {
            TotalTradeCounts = Trades.Count;
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

    public async Task UpdateFilteredTradesAsync()
    {
        if (Trades == null)
        {
            return;
        }

        FilteringIsRunningIconVisibility = Visibility.Visible;

        if (_cancellationTokenSource is not null)
        {
            await _cancellationTokenSource.CancelAsync();
        }

        _cancellationTokenSource = new CancellationTokenSource();

        try
        {
            List<Trade> filteredTrades;
            if (Trades.Count <= 0)
            {
                filteredTrades = [];
            }
            else
            {
                filteredTrades = await Task.Run(ParallelTradeFilterProcess, _cancellationTokenSource.Token);
            }

            TradeCollectionView ??= CollectionViewSource.GetDefaultView(Trades) as ListCollectionView;

            if (TradeCollectionView != null)
            {
                var filteredTradeSet = filteredTrades.ToHashSet();
                TradeCollectionView.Filter = obj => obj is Trade trade && filteredTradeSet.Contains(trade);
                TradeStatsObject?.SetTradeStats(TradeCollectionView.Cast<Trade>().ToList());
            }

            await UpdateProfitOverTimeChartAsync(filteredTrades);
            UpdateCurrentTradesUi(null, null);
        }
        catch (TaskCanceledException)
        {
            // Ignored
        }
        finally
        {
            FilteringIsRunningIconVisibility = Visibility.Collapsed;
        }
    }

    public List<Trade> ParallelTradeFilterProcess()
    {
        var context = BuildFilterContext();
        var partitioner = Partitioner.Create(Trades, EnumerablePartitionerOptions.NoBuffering);
        var result = new ConcurrentBag<Trade>();

        Parallel.ForEach(partitioner, (tradeBatch, state) =>
        {
            if (_cancellationTokenSource.Token.IsCancellationRequested)
            {
                FilteringIsRunningIconVisibility = Visibility.Collapsed;
                state.Stop();
            }

            if (Filter(tradeBatch, context))
            {
                result.Add(tradeBatch);
            }
        });

        return result.OrderByDescending(d => d.Ticks).ToList();
    }

    public async Task UpdateProfitOverTimeChartAsync(IEnumerable<Trade> filteredTrades = null)
    {
        var tradeSnapshot = filteredTrades?.ToList() ?? GetFilteredTradeSnapshot();
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

    private TradeFilterContext BuildFilterContext()
    {
        var searchText = TradesSearchText?.Trim() ?? string.Empty;
        var hasNumericSearch = long.TryParse(searchText, NumberStyles.Any, CultureInfo.CurrentCulture, out var searchNumber);
        var toDate = DatePickerTradeTo.Date;
        var toTicks = toDate == DateTime.MaxValue.Date ? DateTime.MaxValue.Ticks : toDate.AddDays(1).AddTicks(-1).Ticks;

        return new TradeFilterContext(
            DatePickerTradeFrom.Ticks,
            toTicks,
            searchText,
            hasNumericSearch ? searchNumber : null,
            SelectedTierFilter,
            SelectedLocationFilter);
    }

    private bool Filter(object obj, TradeFilterContext context)
    {
        if (obj is not Trade trade)
        {
            return false;
        }

        if (trade.Ticks < context.FromTicks || trade.Ticks > context.ToTicks)
        {
            return false;
        }

        if (!MatchesTierFilter(trade, context.Tier))
        {
            return false;
        }

        if (!MatchesLocationFilter(trade, context.Location))
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
                   trade.InstantBuySellContent?.TotalPrice.IntegerValue == searchNumber;
        }

        return (trade.LocationName?.IndexOf(context.SearchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
               ($"T{trade.Item?.Tier}.{trade.Item?.Level}".IndexOf(context.SearchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
               (trade.MailTypeDescription?.IndexOf(context.SearchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
               (trade.Item?.LocalizedName?.IndexOf(context.SearchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
               (trade.MailContent?.UnitPriceWithoutTax.ToString().IndexOf(context.SearchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
               (trade.MailContent?.TotalPrice.ToString().IndexOf(context.SearchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
               (trade.InstantBuySellContent?.UnitPrice.ToString().IndexOf(context.SearchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
               (trade.InstantBuySellContent?.TotalPrice.ToString().IndexOf(context.SearchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
               (trade.Description?.IndexOf(context.SearchText, StringComparison.OrdinalIgnoreCase) >= 0);
    }

    private static IReadOnlyList<KeyValuePair<int, string>> BuildTierFilters()
    {
        return new List<KeyValuePair<int, string>>
        {
            new(0, LocalizationController.Translation("ALL")),
            new(1, "T1"),
            new(2, "T2"),
            new(3, "T3"),
            new(4, "T4"),
            new(5, "T5"),
            new(6, "T6"),
            new(7, "T7"),
            new(8, "T8")
        };
    }

    private static IReadOnlyList<KeyValuePair<MarketLocation, string>> BuildLocationFilters()
    {
        var filters = new List<KeyValuePair<MarketLocation, string>>
        {
            new(MarketLocation.Unknown, LocalizationController.Translation("ALL"))
        };

        filters.AddRange(Locations.OnceMarketLocations.Select(location => new KeyValuePair<MarketLocation, string>(location.Key, location.Value)));

        return filters;
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

    private static bool MatchesTierFilter(Trade trade, int selectedTier)
    {
        if (selectedTier <= 0)
        {
            return true;
        }

        var itemTier = trade.Item?.Tier ?? 0;
        return itemTier == selectedTier;
    }

    private static bool MatchesLocationFilter(Trade trade, MarketLocation selectedLocation)
    {
        if (selectedLocation == MarketLocation.Unknown)
        {
            return true;
        }

        return trade.Location == selectedLocation;
    }

    private List<Trade> GetFilteredTradeSnapshot()
    {
        if (TradeCollectionView == null)
        {
            return Trades?.ToList() ?? [];
        }

        return TradeCollectionView.Cast<Trade>().ToList();
    }

    private void ApplyProfitOverTimeChart(TradeProfitTimeSeriesResult chartResult)
    {
        _profitOverTimePoints = chartResult.Points ?? [];
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

    private ISeries CreateProfitOverTimeSeries(bool isPositiveSeries, string resourceKey)
    {
        var values = new ObservableCollection<double>();

        for (var i = 0; i < _profitOverTimePoints.Count; i++)
        {
            var point = _profitOverTimePoints[i];
            var value = point.NetProfit;
            values.Add(isPositiveSeries
                ? Math.Max(0d, value)
                : Math.Min(0d, value));
        }

        var fill = CreatePaint(resourceKey);

        return new ColumnSeries<double>
        {
            Name = string.Empty,
            Values = values,
            Stroke = null,
            Fill = fill,
            MaxBarWidth = 6,
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
            labels[i] = i % labelStep == 0 || i == points.Count - 1
                ? FormatAxisLabel(points[i], aggregation, bucketStepSize)
                : string.Empty;
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
            TradeProfitTimeAggregation.Month => point.PeriodStart.ToString("MM.yy", CultureInfo.CurrentCulture),
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
