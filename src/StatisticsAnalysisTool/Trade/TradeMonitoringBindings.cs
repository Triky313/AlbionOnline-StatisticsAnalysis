using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Properties;
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;

namespace StatisticsAnalysisTool.Trade;

public class TradeMonitoringBindings : INotifyPropertyChanged
{
    private readonly ListCollectionView _tradeCollectionView;
    private ObservableRangeCollection<Trade> _trade = new();
    private string _tradesSearchText;
    private DateTime _datePickerTradeFrom = new(2017, 1, 1);
    private DateTime _datePickerTradeTo = DateTime.UtcNow.AddDays(1);
    private TradeStatsObject _tradeStatsObject = new();
    private MailOptionsObject _mailOptionsObject = new();
    private Visibility _isMailMonitoringPopupVisible = Visibility.Collapsed;
    private GridLength _gridSplitterPosition = GridLength.Auto;
    private int _totalTrades;
    private int _currentMails;

    public TradeMonitoringBindings()
    {
        TradeCollectionView = CollectionViewSource.GetDefaultView(Trade) as ListCollectionView;

        if (TradeCollectionView != null)
        {
            TradeCollectionView.CurrentChanged += UpdateCurrentMailsUi;
            Trade.CollectionChanged += UpdateTotalMailsUi;

            TradeCollectionView.IsLiveSorting = true;
            TradeCollectionView.IsLiveFiltering = true;
            TradeCollectionView.CustomSort = new TradeComparer();

            TradeCollectionView.Filter = Filter;
            TradeCollectionView?.Refresh();
        }
    }

    public ListCollectionView TradeCollectionView
    {
        get => _tradeCollectionView;
        init
        {
            _tradeCollectionView = value;
            OnPropertyChanged();
        }
    }

    public ObservableRangeCollection<Trade> Trade
    {
        get => _trade;
        set
        {
            _trade = value;
            OnPropertyChanged();
        }
    }

    public string TradesSearchText
    {
        get => _tradesSearchText;
        set
        {
            _tradesSearchText = value;
            TradeCollectionView?.Refresh();
            TradeStatsObject.SetMailStats(TradeCollectionView?.Cast<Trade>().ToList());
            OnPropertyChanged();
        }
    }

    public DateTime DatePickerTradeFrom
    {
        get => _datePickerTradeFrom;
        set
        {
            _datePickerTradeFrom = value;
            TradeCollectionView?.Refresh();
            TradeStatsObject.SetMailStats(TradeCollectionView?.Cast<Trade>().ToList());
            OnPropertyChanged();
        }
    }

    public DateTime DatePickerTradeTo
    {
        get => _datePickerTradeTo;
        set
        {
            _datePickerTradeTo = value;
            TradeCollectionView?.Refresh();
            TradeStatsObject.SetMailStats(TradeCollectionView?.Cast<Trade>().ToList());
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

    public MailOptionsObject MailOptionsObject
    {
        get => _mailOptionsObject;
        set
        {
            _mailOptionsObject = value;
            OnPropertyChanged();
        }
    }

    public int TotalTrades
    {
        get => _totalTrades;
        set
        {
            _totalTrades = value;
            OnPropertyChanged();
        }
    }

    public int CurrentMails
    {
        get => _currentMails;
        set
        {
            _currentMails = value;
            OnPropertyChanged();
        }
    }

    public Visibility IsMailMonitoringPopupVisible
    {
        get => _isMailMonitoringPopupVisible;
        set
        {
            _isMailMonitoringPopupVisible = value;
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

    #region Update ui

    public void UpdateTotalMailsUi(object sender, NotifyCollectionChangedEventArgs e)
    {
        Application.Current.Dispatcher.InvokeAsync(() =>
        {
            TotalTrades = Trade.Count;
        });
    }

    public void UpdateCurrentMailsUi(object sender, EventArgs e)
    {
        Application.Current.Dispatcher.InvokeAsync(() =>
        {
            CurrentMails = TradeCollectionView.Count;
        });
    }

    #endregion

    #region Filter

    private bool Filter(object obj)
    {
        return true;
        // TODO: Filter erweitern
        //return obj is Mails.Mail mail
        //       && mail.Timestamp.Date >= DatePickerTradeFrom.Date
        //       && mail.Timestamp.Date <= DatePickerTradeTo.Date && (
        //           mail.LocationName != null && mail.LocationName.ToLower().Contains(TradesSearchText?.ToLower() ?? string.Empty)
        //           || $"T{mail.Item.Tier}.{mail.Item.Level}".ToLower().Contains(TradesSearchText?.ToLower() ?? string.Empty)
        //           || mail.MailTypeDescription.ToLower().Contains(TradesSearchText?.ToLower() ?? string.Empty)
        //           || mail.Item != null && mail.Item.LocalizedName.ToLower().Contains(TradesSearchText?.ToLower() ?? string.Empty)
        //           || mail.MailContent.ActualUnitPrice.ToString().Contains(TradesSearchText?.ToLower() ?? string.Empty)
        //           || mail.MailContent.TotalPrice.ToString().Contains(TradesSearchText?.ToLower() ?? string.Empty));
    }

    #endregion

    public event PropertyChangedEventHandler PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}