using log4net;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace StatisticsAnalysisTool.Models;

public class MarketQualityObject : INotifyPropertyChanged
{
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

    private Visibility _visibility;
    private MarketLocation _marketLocation;
    private ulong _sellPriceMinNormal;
    private ulong _sellPriceMinGood;
    private ulong _sellPriceMinOutstanding;
    private ulong _sellPriceMinExcellent;
    private ulong _sellPriceMinMasterpiece;
    private DateTime _sellPriceMinNormalDate;
    private DateTime _sellPriceMinGoodDate;
    private DateTime _sellPriceMinOutstandingDate;
    private DateTime _sellPriceMinExcellentDate;
    private DateTime _sellPriceMinMasterpieceDate;

    public MarketQualityObject(MarketResponse marketResponse)
    {
        MarketLocation = (marketResponse?.City ?? string.Empty).GetMarketLocationByLocationNameOrId();

        if (marketResponse == null)
        {
            return;
        }

        SetValues(marketResponse);
    }

    public string LocationName => Locations.GetDisplayName(MarketLocation);

    public Visibility Visibility
    {
        get => _visibility;
        set
        {
            _visibility = value;
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

    public ulong SellPriceMinNormal
    {
        get => _sellPriceMinNormal;
        set
        {
            _sellPriceMinNormal = value;
            OnPropertyChanged();
        }
    }

    public ulong SellPriceMinGood
    {
        get => _sellPriceMinGood;
        set
        {
            _sellPriceMinGood = value;
            OnPropertyChanged();
        }
    }

    public ulong SellPriceMinOutstanding
    {
        get => _sellPriceMinOutstanding;
        set
        {
            _sellPriceMinOutstanding = value;
            OnPropertyChanged();
        }
    }

    public ulong SellPriceMinExcellent
    {
        get => _sellPriceMinExcellent;
        set
        {
            _sellPriceMinExcellent = value;
            OnPropertyChanged();
        }
    }

    public ulong SellPriceMinMasterpiece
    {
        get => _sellPriceMinMasterpiece;
        set
        {
            _sellPriceMinMasterpiece = value;
            OnPropertyChanged();
        }
    }

    public DateTime SellPriceMinNormalDate
    {
        get => _sellPriceMinNormalDate;
        set
        {
            _sellPriceMinNormalDate = value;
            OnPropertyChanged();
        }
    }

    public DateTime SellPriceMinGoodDate
    {
        get => _sellPriceMinGoodDate;
        set
        {
            _sellPriceMinGoodDate = value;
            OnPropertyChanged();
        }
    }

    public DateTime SellPriceMinOutstandingDate
    {
        get => _sellPriceMinOutstandingDate;
        set
        {
            _sellPriceMinOutstandingDate = value;
            OnPropertyChanged();
        }
    }

    public DateTime SellPriceMinExcellentDate
    {
        get => _sellPriceMinExcellentDate;
        set
        {
            _sellPriceMinExcellentDate = value;
            OnPropertyChanged();
        }
    }

    public DateTime SellPriceMinMasterpieceDate
    {
        get => _sellPriceMinMasterpieceDate;
        set
        {
            _sellPriceMinMasterpieceDate = value;
            OnPropertyChanged();
        }
    }

    public bool IsSellPriceMinNormalBestPrice => BestMinPrice() == BestPriceQuality.Normal;
    public bool IsSellPriceMinGoodBestPrice => BestMinPrice() == BestPriceQuality.Good;
    public bool IsSellPriceMinOutstandingBestPrice => BestMinPrice() == BestPriceQuality.Outstanding;
    public bool IsSellPriceMinExcellentBestPrice => BestMinPrice() == BestPriceQuality.Excellent;
    public bool IsSellPriceMinMasterpieceBestPrice => BestMinPrice() == BestPriceQuality.Masterpiece;

    private BestPriceQuality BestMinPrice()
    {
        var priceValues = new List<ulong>
        {
            SellPriceMinNormal,
            SellPriceMinGood,
            SellPriceMinOutstanding,
            SellPriceMinExcellent,
            SellPriceMinMasterpiece
        };
        var minPrice = ItemController.GetMinPrice(priceValues);

        if (minPrice == SellPriceMinNormal)
        {
            return BestPriceQuality.Normal;
        }

        if (minPrice == SellPriceMinGood)
        {
            return BestPriceQuality.Good;
        }

        if (minPrice == SellPriceMinOutstanding)
        {
            return BestPriceQuality.Outstanding;
        }

        if (minPrice == SellPriceMinExcellent)
        {
            return BestPriceQuality.Excellent;
        }

        if (minPrice == SellPriceMinMasterpiece)
        {
            return BestPriceQuality.Masterpiece;
        }

        return BestPriceQuality.Unknown;
    }

    public void SetValues(MarketResponse marketResponse)
    {
        switch (ItemController.GetQuality(marketResponse.QualityLevel))
        {
            case ItemQuality.Normal:
                SellPriceMinNormal = marketResponse.SellPriceMin;
                SellPriceMinNormalDate = marketResponse.SellPriceMinDate;
                return;

            case ItemQuality.Good:
                SellPriceMinGood = marketResponse.SellPriceMin;
                SellPriceMinGoodDate = marketResponse.SellPriceMinDate;
                return;

            case ItemQuality.Outstanding:
                SellPriceMinOutstanding = marketResponse.SellPriceMin;
                SellPriceMinOutstandingDate = marketResponse.SellPriceMinDate;
                return;

            case ItemQuality.Excellent:
                SellPriceMinExcellent = marketResponse.SellPriceMin;
                SellPriceMinExcellentDate = marketResponse.SellPriceMinDate;
                return;

            case ItemQuality.Masterpiece:
                SellPriceMinMasterpiece = marketResponse.SellPriceMin;
                SellPriceMinMasterpieceDate = marketResponse.SellPriceMinDate;
                return;
        }
    }
    
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