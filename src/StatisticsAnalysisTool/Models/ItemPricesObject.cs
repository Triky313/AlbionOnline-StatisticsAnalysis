using Serilog;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using StatisticsAnalysisTool.Diagnostics;

namespace StatisticsAnalysisTool.Models;

public class ItemPricesObject : BaseViewModel
{
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
    private ValueTimeStatus _sellPriceMinDateStatus;
    private string _sellPriceMinDateString;
    private string _sellPriceMinDateLastUpdateTime;
    private ValueTimeStatus _sellPriceMaxDateStatus;
    private string _sellPriceMaxDateString;
    private string _sellPriceMaxDateLastUpdateTime;
    private ValueTimeStatus _buyPriceMinDateStatus;
    private string _buyPriceMinDateString;
    private string _buyPriceMinDateLastUpdateTime;
    private ValueTimeStatus _buyPriceMaxDateStatus;
    private string _buyPriceMaxDateString;
    private string _buyPriceMaxDateLastUpdateTime;

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

    public string LocationName => Locations.GetDisplayName(MarketLocation);

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
            SellPriceMinDateStatus = value.GetValueTimeStatus();
            SellPriceMinDateString = value.CurrentDateTimeFormat();
            SellPriceMinDateLastUpdateTime = value.DateTimeToLastUpdateTime();
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
            SellPriceMaxDateStatus = value.GetValueTimeStatus();
            SellPriceMaxDateString = value.CurrentDateTimeFormat();
            SellPriceMaxDateLastUpdateTime = value.DateTimeToLastUpdateTime();
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
            BuyPriceMinDateStatus = value.GetValueTimeStatus();
            BuyPriceMinDateString = value.CurrentDateTimeFormat();
            BuyPriceMinDateLastUpdateTime = value.DateTimeToLastUpdateTime();
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
            BuyPriceMaxDateStatus = value.GetValueTimeStatus();
            BuyPriceMaxDateString = value.CurrentDateTimeFormat();
            BuyPriceMaxDateLastUpdateTime = value.DateTimeToLastUpdateTime();
            OnPropertyChanged();
        }
    }

    #endregion

    #region Modified values

    public ValueTimeStatus SellPriceMinDateStatus
    {
        get => _sellPriceMinDateStatus;
        set
        {
            _sellPriceMinDateStatus = value;
            OnPropertyChanged();
        }
    }

    public string SellPriceMinDateString
    {
        get => _sellPriceMinDateString;
        set
        {
            _sellPriceMinDateString = value;
            OnPropertyChanged();
        }
    }

    public string SellPriceMinDateLastUpdateTime
    {
        get => _sellPriceMinDateLastUpdateTime;
        set
        {
            _sellPriceMinDateLastUpdateTime = value;
            OnPropertyChanged();
        }
    }

    public ValueTimeStatus SellPriceMaxDateStatus
    {
        get => _sellPriceMaxDateStatus;
        set
        {
            _sellPriceMaxDateStatus = value;
            OnPropertyChanged();
        }
    }

    public string SellPriceMaxDateString
    {
        get => _sellPriceMaxDateString;
        set
        {
            _sellPriceMaxDateString = value;
            OnPropertyChanged();
        }
    }

    public string SellPriceMaxDateLastUpdateTime
    {
        get => _sellPriceMaxDateLastUpdateTime;
        set
        {
            _sellPriceMaxDateLastUpdateTime = value;
            OnPropertyChanged();
        }
    }

    public ValueTimeStatus BuyPriceMinDateStatus
    {
        get => _buyPriceMinDateStatus;
        set
        {
            _buyPriceMinDateStatus = value;
            OnPropertyChanged();
        }
    }

    public string BuyPriceMinDateString
    {
        get => _buyPriceMinDateString;
        set
        {
            _buyPriceMinDateString = value;
            OnPropertyChanged();
        }
    }

    public string BuyPriceMinDateLastUpdateTime
    {
        get => _buyPriceMinDateLastUpdateTime;
        set
        {
            _buyPriceMinDateLastUpdateTime = value;
            OnPropertyChanged();
        }
    }

    public ValueTimeStatus BuyPriceMaxDateStatus
    {
        get => _buyPriceMaxDateStatus;
        set
        {
            _buyPriceMaxDateStatus = value;
            OnPropertyChanged();
        }
    }

    public string BuyPriceMaxDateString
    {
        get => _buyPriceMaxDateString;
        set
        {
            _buyPriceMaxDateString = value;
            OnPropertyChanged();
        }
    }

    public string BuyPriceMaxDateLastUpdateTime
    {
        get => _buyPriceMaxDateLastUpdateTime;
        set
        {
            _buyPriceMaxDateLastUpdateTime = value;
            OnPropertyChanged();
        }
    }

    #endregion

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
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, ex);
            Log.Error(ex, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
        }
    }
}