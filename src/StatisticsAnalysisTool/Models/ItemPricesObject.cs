using log4net;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.GameData;
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
    private readonly MarketResponse _marketResponse;
    private readonly MarketLocation _marketLocation = MarketLocation.Unknown;
    private bool _bestSellMinPrice;
    private bool _bestSellMaxPrice;
    private bool _bestBuyMinPrice;
    private bool _bestBuyMaxPrice;

    public ItemPricesObject(MarketResponse marketResponse)
    {
        MarketResponse = marketResponse;
        MarketLocation = (marketResponse?.City ?? string.Empty).GetMarketLocationByLocationNameOrId();
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

    public bool BestSellMinPrice
    {
        get => _bestSellMinPrice;
        set
        {
            _bestSellMinPrice = value;
            OnPropertyChanged();
        }
    }

    public bool BestSellMaxPrice
    {
        get => _bestSellMaxPrice;
        set
        {
            _bestSellMaxPrice = value;
            OnPropertyChanged();
        }
    }

    public bool BestBuyMinPrice
    {
        get => _bestBuyMinPrice;
        set
        {
            _bestBuyMinPrice = value;
            OnPropertyChanged();
        }
    }

    public bool BestBuyMaxPrice
    {
        get => _bestBuyMaxPrice;
        set
        {
            _bestBuyMaxPrice = value;
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