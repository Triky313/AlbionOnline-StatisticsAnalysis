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
    private MarketLocation _marketLocation = MarketLocation.Unknown;
    private bool _isBestSellMinPrice;
    private bool _isBestSellMaxPrice;
    private bool _isBestBuyMinPrice;
    private bool _isBestBuyMaxPrice;
    private string _itemTypeId;
    private int _qualityLevel;
    private ulong _sellPriceMin;
    private DateTime _sellPriceMinDate;
    private ulong _sellPriceMax;
    private DateTime _sellPriceMaxDate;
    private ulong _buyPriceMin;
    private DateTime _buyPriceMinDate;
    private ulong _buyPriceMax;
    private DateTime _buyPriceMaxDate;

    public ItemPricesObject(MarketResponse marketResponse)
    {
        ItemTypeId = marketResponse?.ItemTypeId ?? string.Empty;
        MarketLocation = (marketResponse?.City ?? string.Empty).GetMarketLocationByLocationNameOrId();

        if (marketResponse == null)
        {
            return;
        }
        
        QualityLevel = marketResponse.QualityLevel;
        SellPriceMin = marketResponse.SellPriceMin;
        SellPriceMinDate = marketResponse.SellPriceMinDate;
        SellPriceMax = marketResponse.SellPriceMax;
        SellPriceMaxDate = marketResponse.SellPriceMaxDate;
        BuyPriceMin = marketResponse.BuyPriceMin;
        BuyPriceMinDate = marketResponse.BuyPriceMinDate;
        BuyPriceMax = marketResponse.BuyPriceMax;
        BuyPriceMaxDate = marketResponse.BuyPriceMaxDate;
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

    #region Best values

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

    #endregion

    #region Market response values

    public string ItemTypeId
    {
        get => _itemTypeId;
        set
        {
            _itemTypeId = value;
            OnPropertyChanged();
        }
    }

    public MarketLocation MarketLocation
    {
        get => _marketLocation;
        set
        {
            _marketLocation = value;
            OnPropertyChanged();
        }
    }

    public int QualityLevel
    {
        get => _qualityLevel;
        set
        {
            _qualityLevel = value;
            OnPropertyChanged();
        }
    }

    public ulong SellPriceMin
    {
        get => _sellPriceMin;
        set
        {
            _sellPriceMin = value;
            OnPropertyChanged();
        }
    }

    public DateTime SellPriceMinDate
    {
        get => _sellPriceMinDate;
        set
        {
            _sellPriceMinDate = value;
            OnPropertyChanged();
        }
    }

    public ulong SellPriceMax
    {
        get => _sellPriceMax;
        set
        {
            _sellPriceMax = value;
            OnPropertyChanged();
        }
    }

    public DateTime SellPriceMaxDate
    {
        get => _sellPriceMaxDate;
        set
        {
            _sellPriceMaxDate = value;
            OnPropertyChanged();
        }
    }

    public ulong BuyPriceMin
    {
        get => _buyPriceMin;
        set
        {
            _buyPriceMin = value;
            OnPropertyChanged();
        }
    }

    public DateTime BuyPriceMinDate
    {
        get => _buyPriceMinDate;
        set
        {
            _buyPriceMinDate = value;
            OnPropertyChanged();
        }
    }

    public ulong BuyPriceMax
    {
        get => _buyPriceMax;
        set
        {
            _buyPriceMax = value;
            OnPropertyChanged();
        }
    }

    public DateTime BuyPriceMaxDate
    {
        get => _buyPriceMaxDate;
        set
        {
            _buyPriceMaxDate = value;
            OnPropertyChanged();
        }
    }

    #endregion

    public string LocationName => Locations.GetDisplayName(MarketLocation);
    public ValueTimeStatus SellPriceMinDateStatus => SellPriceMinDate.GetValueTimeStatus();
    public string SellPriceMinDateString => SellPriceMinDate.CurrentDateTimeFormat();
    public string SellPriceMinDateLastUpdateTime => SellPriceMinDate.DateTimeToLastUpdateTime();
    public ValueTimeStatus SellPriceMaxDateStatus => SellPriceMaxDate.GetValueTimeStatus();
    public string SellPriceMaxDateString => SellPriceMaxDate.CurrentDateTimeFormat();
    public string SellPriceMaxDateLastUpdateTime => SellPriceMaxDate.DateTimeToLastUpdateTime();
    public ValueTimeStatus BuyPriceMinDateStatus => BuyPriceMinDate.GetValueTimeStatus();
    public string BuyPriceMinDateString => BuyPriceMinDate.CurrentDateTimeFormat();
    public string BuyPriceMinDateLastUpdateTime => BuyPriceMinDate.DateTimeToLastUpdateTime();
    public ValueTimeStatus BuyPriceMaxDateStatus => BuyPriceMaxDate.GetValueTimeStatus();
    public string BuyPriceMaxDateString => BuyPriceMaxDate.CurrentDateTimeFormat();
    public string BuyPriceMaxDateLastUpdateTime => BuyPriceMaxDate.DateTimeToLastUpdateTime();

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