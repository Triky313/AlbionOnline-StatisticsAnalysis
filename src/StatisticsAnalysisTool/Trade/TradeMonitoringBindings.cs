using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace StatisticsAnalysisTool.Trade;

public class TradeMonitoringBindings : BaseViewModel
{
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

    public TradeMonitoringBindings()
    {
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
        _datePickerTradeFrom = new DateTime(2017, 1, 1);
        _datePickerTradeTo = DateTime.UtcNow.AddDays(1);
        TradesSearchText = string.Empty;

        TradeCollectionView = CollectionViewSource.GetDefaultView(Trades) as ListCollectionView;
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
            CurrentTradeCounts = TradeCollectionView.Count;
        });
    }

    #endregion

    #region Filter

    private CancellationTokenSource _cancellationTokenSource;

    public async Task UpdateFilteredTradesAsync()
    {
        if (Trades?.Count <= 0 && TradeCollectionView?.Count <= 0)
        {
            return;
        }

        FilteringIsRunningIconVisibility = Visibility.Visible;

        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource = new CancellationTokenSource();

        try
        {
            var filteredTrades = await Task.Run(ParallelTradeFilterProcess, _cancellationTokenSource.Token);

            TradeCollectionView = CollectionViewSource.GetDefaultView(filteredTrades) as ListCollectionView;
            TradeStatsObject?.SetTradeStats(TradeCollectionView?.Cast<Trade>().ToList());
            UpdateCurrentTradesUi(null, null);
        }
        catch (TaskCanceledException)
        {
            // ignored
        }
        finally
        {
            FilteringIsRunningIconVisibility = Visibility.Collapsed;
        }
    }

    public List<Trade> ParallelTradeFilterProcess()
    {
        var partitioner = Partitioner.Create(Trades, EnumerablePartitionerOptions.NoBuffering);
        var result = new ConcurrentBag<Trade>();

        Parallel.ForEach(partitioner, (tradeBatch, state) =>
        {
            if (_cancellationTokenSource.Token.IsCancellationRequested)
            {
                FilteringIsRunningIconVisibility = Visibility.Collapsed;
                state.Stop();
            }

            if (Filter(tradeBatch))
            {
                result.Add(tradeBatch);
            }
        });

        return result.OrderByDescending(d => d.Ticks).ToList();
    }

    private bool Filter(object obj)
    {
        if (obj is null)
        {
            return false;
        }

        return obj is Trade trade2 &&
               (TradesSearchText == null &&
                trade2.Timestamp.Date >= DatePickerTradeFrom.Date &&
                trade2.Timestamp.Date <= DatePickerTradeTo.Date)
               ||
               (obj is Trade trade &&
                TradesSearchText != null &&
                trade.Timestamp.Date >= DatePickerTradeFrom.Date &&
                trade.Timestamp.Date <= DatePickerTradeTo.Date &&
                (
                    trade.LocationName != null && trade.LocationName.IndexOf(TradesSearchText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    $"T{trade.Item?.Tier}.{trade.Item?.Level}".IndexOf(TradesSearchText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    trade.MailTypeDescription.IndexOf(TradesSearchText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    trade.Item != null && trade.Item.LocalizedName.IndexOf(TradesSearchText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    trade.MailContent.ActualUnitPrice.ToString().IndexOf(TradesSearchText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    trade.MailContent.TotalPrice.ToString().IndexOf(TradesSearchText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    trade.InstantBuySellContent.UnitPrice.ToString().IndexOf(TradesSearchText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    trade.InstantBuySellContent.TotalPrice.ToString().IndexOf(TradesSearchText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    trade.Description?.IndexOf(TradesSearchText, StringComparison.OrdinalIgnoreCase) >= 0
                ));
    }

    #endregion
}