using log4net;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.GameData;
using System;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using StatisticsAnalysisTool.Enumerations;

namespace StatisticsAnalysisTool.Models;

public class MarketCurrentPricesItem
{
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

    public MarketCurrentPricesItem(MarketResponseTotal marketResponseTotal)
    {
        ItemTypeId = marketResponseTotal.ItemTypeId;
        LocationEnumType = marketResponseTotal.City;
        QualityLevel = marketResponseTotal.QualityLevel;
        SellPriceMin = marketResponseTotal.SellPriceMin;
        SellPriceMinDate = marketResponseTotal.SellPriceMinDate;
        SellPriceMax = marketResponseTotal.SellPriceMax;
        SellPriceMaxDate = marketResponseTotal.SellPriceMaxDate;
        BuyPriceMin = marketResponseTotal.BuyPriceMin;
        BuyPriceMinDate = marketResponseTotal.BuyPriceMinDate;
        BuyPriceMax = marketResponseTotal.BuyPriceMax;
        BuyPriceMaxDate = marketResponseTotal.BuyPriceMaxDate;
        BestSellMinPrice = marketResponseTotal.BestSellMinPrice;
        BestSellMaxPrice = marketResponseTotal.BestSellMaxPrice;
        BestBuyMinPrice = marketResponseTotal.BestBuyMinPrice;
        BestBuyMaxPrice = marketResponseTotal.BestBuyMaxPrice;
    }

    public string ItemTypeId { get; set; }
    public Location LocationEnumType { get; set; }
    public string LocationName => WorldData.GetUniqueNameOrDefault((int)LocationEnumType);
    public byte QualityLevel { get; set; }
    public ulong SellPriceMin { get; set; }
    public string SellPriceMinString => Utilities.UlongMarketPriceToString(SellPriceMin);
    public DateTime SellPriceMinDate { get; set; }
    public ValueTimeStatus SellPriceMinDateStatus => SellPriceMinDate.GetValueTimeStatus();
    public string SellPriceMinDateString => SellPriceMinDate.CurrentDateTimeFormat();
    public string SellPriceMinDateLastUpdateTime => SellPriceMinDate.DateTimeToLastUpdateTime();
    public ulong SellPriceMax { get; set; }
    public string SellPriceMaxString => Utilities.UlongMarketPriceToString(SellPriceMax);
    public DateTime SellPriceMaxDate { get; set; }
    public ValueTimeStatus SellPriceMaxDateStatus => SellPriceMaxDate.GetValueTimeStatus();
    public string SellPriceMaxDateString => SellPriceMaxDate.CurrentDateTimeFormat();
    public string SellPriceMaxDateLastUpdateTime => SellPriceMaxDate.DateTimeToLastUpdateTime();
    public ulong BuyPriceMin { get; set; }
    public string BuyPriceMinString => Utilities.UlongMarketPriceToString(BuyPriceMin);
    public DateTime BuyPriceMinDate { get; set; }
    public ValueTimeStatus BuyPriceMinDateStatus => BuyPriceMinDate.GetValueTimeStatus();
    public string BuyPriceMinDateString => BuyPriceMinDate.CurrentDateTimeFormat();
    public string BuyPriceMinDateLastUpdateTime => BuyPriceMinDate.DateTimeToLastUpdateTime();
    public ulong BuyPriceMax { get; set; }
    public string BuyPriceMaxString => Utilities.UlongMarketPriceToString(BuyPriceMax);
    public DateTime BuyPriceMaxDate { get; set; }
    public ValueTimeStatus BuyPriceMaxDateStatus => BuyPriceMaxDate.GetValueTimeStatus();
    public string BuyPriceMaxDateString => BuyPriceMaxDate.CurrentDateTimeFormat();
    public string BuyPriceMaxDateLastUpdateTime => BuyPriceMaxDate.DateTimeToLastUpdateTime();
    public bool BestSellMinPrice { get; set; }
    public bool BestSellMaxPrice { get; set; }
    public bool BestBuyMinPrice { get; set; }
    public bool BestBuyMaxPrice { get; set; }

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