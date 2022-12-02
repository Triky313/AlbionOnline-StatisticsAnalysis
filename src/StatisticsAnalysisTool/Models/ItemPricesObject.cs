using log4net;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using System;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace StatisticsAnalysisTool.Models;

public class ItemPricesObject : INotifyPropertyChanged
{
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);
    private Visibility _visibility;
    private readonly MarketResponse _marketResponse;
    private readonly MarketLocation _marketLocation = MarketLocation.Unknown;
    private bool _isBestSellMinPrice;
    private bool _isBestSellMaxPrice;
    private bool _isBestBuyMinPrice;
    private bool _isBestBuyMaxPrice;

    public ItemPricesObject(MarketResponse marketResponse)
    {
        MarketResponse = marketResponse;
        MarketLocation = (marketResponse?.City ?? string.Empty).GetMarketLocationByLocationNameOrId();
    }

    public Visibility Visibility
    {
        get => _visibility;
        set
        {
            _visibility = value;
            OnPropertyChanged();
        }
    }

    public MarketResponse MarketResponse
    {
        get => _marketResponse;
        init
        {
            _marketResponse = value;
            OnPropertyChanged();
        }
    }

    public MarketLocation MarketLocation
    {
        get => _marketLocation;
        init
        {
            _marketLocation = value;
            OnPropertyChanged();
        }
    }

    public bool IsBestSellMinPrice
    {
        get => _isBestSellMinPrice;
        set
        {
            _isBestSellMinPrice = value;
            OnPropertyChanged();
        }
    }

    public bool IsBestSellMaxPrice
    {
        get => _isBestSellMaxPrice;
        set
        {
            _isBestSellMaxPrice = value;
            OnPropertyChanged();
        }
    }

    public bool IsBestBuyMinPrice
    {
        get => _isBestBuyMinPrice;
        set
        {
            _isBestBuyMinPrice = value;
            OnPropertyChanged();
        }
    }

    public bool IsBestBuyMaxPrice
    {
        get => _isBestBuyMaxPrice;
        set
        {
            _isBestBuyMaxPrice = value;
            OnPropertyChanged();
        }
    }

    public string LocationName => Locations.GetDisplayName(MarketLocation);
    public ValueTimeStatus SellPriceMinDateStatus => MarketResponse.SellPriceMinDate.GetValueTimeStatus();
    public string SellPriceMinDateString => MarketResponse.SellPriceMinDate.CurrentDateTimeFormat();
    public string SellPriceMinDateLastUpdateTime => MarketResponse.SellPriceMinDate.DateTimeToLastUpdateTime();
    public ValueTimeStatus SellPriceMaxDateStatus => MarketResponse.SellPriceMaxDate.GetValueTimeStatus();
    public string SellPriceMaxDateString => MarketResponse.SellPriceMaxDate.CurrentDateTimeFormat();
    public string SellPriceMaxDateLastUpdateTime => MarketResponse.SellPriceMaxDate.DateTimeToLastUpdateTime();
    public ValueTimeStatus BuyPriceMinDateStatus => MarketResponse.BuyPriceMinDate.GetValueTimeStatus();
    public string BuyPriceMinDateString => MarketResponse.BuyPriceMinDate.CurrentDateTimeFormat();
    public string BuyPriceMinDateLastUpdateTime => MarketResponse.BuyPriceMinDate.DateTimeToLastUpdateTime();
    public ValueTimeStatus BuyPriceMaxDateStatus => MarketResponse.BuyPriceMaxDate.GetValueTimeStatus();
    public string BuyPriceMaxDateString => MarketResponse.BuyPriceMaxDate.CurrentDateTimeFormat();
    public string BuyPriceMaxDateLastUpdateTime => MarketResponse.BuyPriceMaxDate.DateTimeToLastUpdateTime();

    private ICommand _copyTextToClipboard;
    public ICommand CopyTextToClipboard => _copyTextToClipboard ??= new CommandHandler(PerformCopyTextToClipboard, true);

    private static void PerformCopyTextToClipboard(object param)
    {
        try
        {
            Clipboard.SetText(param as string ?? string.Empty);
        }
        catch (Exception ex)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, ex);
            Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, ex);
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}