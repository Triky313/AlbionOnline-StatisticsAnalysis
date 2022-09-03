using StatisticsAnalysisTool.Common;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using log4net;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.GameData;

namespace StatisticsAnalysisTool.Models;

public class MarketQualityObject
{
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

    public string Location { get; set; }
    public string LocationName => WorldData.GetUniqueNameOrDefault((int)LocationEnumType);
    public Location LocationEnumType => Locations.GetLocationByLocationNameOrId(Location);
    public ulong SellPriceMinNormal { private get; set; }
    public ulong SellPriceMinGood { private get; set; }
    public ulong SellPriceMinOutstanding { private get; set; }
    public ulong SellPriceMinExcellent { private get; set; }
    public ulong SellPriceMinMasterpiece { private get; set; }

    public string SellPriceMinNormalString => Utilities.UlongMarketPriceToString(SellPriceMinNormal);
    public string SellPriceMinGoodString => Utilities.UlongMarketPriceToString(SellPriceMinGood);
    public string SellPriceMinOutstandingString => Utilities.UlongMarketPriceToString(SellPriceMinOutstanding);
    public string SellPriceMinExcellentString => Utilities.UlongMarketPriceToString(SellPriceMinExcellent);
    public string SellPriceMinMasterpieceString => Utilities.UlongMarketPriceToString(SellPriceMinMasterpiece);

    public string SellPriceMinNormalStringInRalMoney { get; set; }
    public string SellPriceMinGoodStringInRalMoney { get; set; }
    public string SellPriceMinOutstandingStringInRalMoney { get; set; }
    public string SellPriceMinExcellentStringInRalMoney { get; set; }
    public string SellPriceMinMasterpieceStringInRalMoney { get; set; }

    public DateTime SellPriceMinNormalDate { private get; set; }
    public DateTime SellPriceMinGoodDate { private get; set; }
    public DateTime SellPriceMinOutstandingDate { private get; set; }
    public DateTime SellPriceMinExcellentDate { private get; set; }
    public DateTime SellPriceMinMasterpieceDate { private get; set; }

    public string SellPriceMinNormalDateString => Utilities.MarketPriceDateToString(SellPriceMinNormalDate);
    public string SellPriceMinGoodDateString => Utilities.MarketPriceDateToString(SellPriceMinGoodDate);
    public string SellPriceMinOutstandingDateString => Utilities.MarketPriceDateToString(SellPriceMinOutstandingDate);
    public string SellPriceMinExcellentDateString => Utilities.MarketPriceDateToString(SellPriceMinExcellentDate);
    public string SellPriceMinMasterpieceDateString => Utilities.MarketPriceDateToString(SellPriceMinMasterpieceDate);
    
    public bool IsSellPriceMinNormalBestPrice => BestMinPrice() == BestPriceQuality.Normal;
    public bool IsSellPriceMinGoodBestPrice => BestMinPrice() == BestPriceQuality.Good;
    public bool IsSellPriceMinOutstandingBestPrice => BestMinPrice() == BestPriceQuality.Outstanding;
    public bool IsSellPriceMinExcellentBestPrice => BestMinPrice() == BestPriceQuality.Excellent;
    public bool IsSellPriceMinMasterpieceBestPrice => BestMinPrice() == BestPriceQuality.Masterpiece;

    private BestPriceQuality BestMinPrice()
    {
        var priceList = new List<ulong>
        {
            SellPriceMinNormal,
            SellPriceMinGood,
            SellPriceMinOutstanding,
            SellPriceMinExcellent,
            SellPriceMinMasterpiece
        };
        var minPrice = ItemController.GetMinPrice(priceList);

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
}