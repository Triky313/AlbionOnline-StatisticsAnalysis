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
    private readonly record struct TradeFilterContext(long FromTicks, long ToTicks, string SearchText, long? SearchNumber, int Tier, int Level, MarketLocation Location);
    private readonly record struct TradeFilterExecutionContext(List<Trade> TradesSnapshot, TradeFilterContext FilterContext);
    private readonly TradeProfitTimeSeriesService _tradeProfitTimeSeriesService = new();
    private readonly TradeProfitTimeOfDayService _tradeProfitTimeOfDayService = new();
    private readonly TradeItemRankingService _tradeItemRankingService = new();
    private IReadOnlyList<TradeProfitTimeSeriesPoint> _profitOverTimePoints = [];
    private IReadOnlyList<TradeProfitTimeOfDayPoint> _profitByTimeOfDayHourlyPoints = [];
    private TradeProfitTimeOfDayResult _profitByTimeOfDayResult = new();

    public TradeMonitoringBindings()
    {
        TierFilters = BuildTierFilters();
        LevelFilters = BuildLevelFilters();
        LocationFilters = BuildLocationFilters();
        ProfitOverTimeAggregationFilters = BuildProfitOverTimeAggregationFilters();
        ProfitByTimeOfDayChartModeFilters = BuildProfitByTimeOfDayChartModeFilters();
        ProfitByTimeOfDayMetricFilters = BuildProfitByTimeOfDayMetricFilters();
        Trades.CollectionChanged += UpdateTotalTradesUi;
        EnsureTradeCollectionViewInitialized();

        DatePickerTradeFrom = SettingsController.CurrentSettings.TradeMonitoringDatePickerTradeFrom;
        DatePickerTradeTo = SettingsController.CurrentSettings.TradeMonitoringDatePickerTradeTo;
        UpdateProfitByTimeOfDayModeVisibility();
    }

    public void ItemFilterReset()
    {
        DatePickerTradeFrom = new DateTime(2017, 1, 1);
        DatePickerTradeTo = DateTime.UtcNow.AddDays(1);
        TradesSearchText = string.Empty;
        SelectedTierFilter = 0;
        SelectedLevelFilter = -1;
        SelectedLocationFilter = MarketLocation.Unknown;

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

    public IReadOnlyList<KeyValuePair<int, string>> TierFilters
    {
        get;
    }

    public int SelectedTierFilter
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public IReadOnlyList<KeyValuePair<int, string>> LevelFilters
    {
        get;
    }

    public int SelectedLevelFilter
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = -1;

    public IReadOnlyList<KeyValuePair<MarketLocation, string>> LocationFilters
    {
        get;
    }

    public MarketLocation SelectedLocationFilter
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = MarketLocation.Unknown;

    public IReadOnlyList<KeyValuePair<TradeProfitTimeAggregation, string>> ProfitOverTimeAggregationFilters
    {
        get;
    }

    public IReadOnlyList<KeyValuePair<TradeTimeOfDayChartMode, string>> ProfitByTimeOfDayChartModeFilters
    {
        get;
    }

    public IReadOnlyList<KeyValuePair<TradeTimeOfDayMetric, string>> ProfitByTimeOfDayMetricFilters
    {
        get;
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
            field = value;
            SettingsController.CurrentSettings.TradeMonitoringDatePickerTradeTo = field;
            OnPropertyChanged();
        }
    } = DateTime.UtcNow.AddDays(1);

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

                var filteredTradeSet = filteredTrades.ToHashSet();
                TradeCollectionView.Filter = obj => obj is Trade trade && filteredTradeSet.Contains(trade);
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

    private async Task UpdateStatisticViewsAsync(IEnumerable<Trade> filteredTrades = null)
    {
        var tradeSnapshot = filteredTrades?.ToList() ?? await GetFilteredTradeSnapshotAsync();

        await Task.WhenAll(
            UpdateProfitOverTimeChartAsync(tradeSnapshot),
            UpdateProfitByTimeOfDayChartAsync(tradeSnapshot),
            UpdateTopItemRankingsAsync(tradeSnapshot));
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
            SelectedLevelFilter,
            SelectedLocationFilter);
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

        if (!MatchesTierFilter(trade, context.Tier))
        {
            return false;
        }

        if (!MatchesLevelFilter(trade, context.Level))
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
               (trade.PlayerTradeContent?.IslandName?.IndexOf(context.SearchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
               (trade.PlayerTradeContent?.Silver.ToString().IndexOf(context.SearchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
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

    private static IReadOnlyList<KeyValuePair<int, string>> BuildLevelFilters()
    {
        var levelText = LocalizationController.Translation("LEVEL");

        return new List<KeyValuePair<int, string>>
        {
            new(-1, LocalizationController.Translation("ALL")),
            new(0, $"{levelText} 0"),
            new(1, $"{levelText} 1"),
            new(2, $"{levelText} 2"),
            new(3, $"{levelText} 3"),
            new(4, $"{levelText} 4")
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

    private static bool MatchesTierFilter(Trade trade, int selectedTier)
    {
        if (selectedTier <= 0)
        {
            return true;
        }

        var itemTier = trade.Item?.Tier ?? 0;
        return itemTier == selectedTier;
    }

    private static bool MatchesLevelFilter(Trade trade, int selectedLevel)
    {
        if (selectedLevel < 0)
        {
            return true;
        }

        return trade.Item?.Level == selectedLevel;
    }

    private static bool MatchesLocationFilter(Trade trade, MarketLocation selectedLocation)
    {
        if (selectedLocation == MarketLocation.Unknown)
        {
            return true;
        }

        return trade.Location == selectedLocation;
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
