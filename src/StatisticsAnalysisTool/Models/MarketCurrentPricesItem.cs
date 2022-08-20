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
    public ValueTimeStatus SellPriceMinDateStatus => GetValueTimeStatus(SellPriceMinDate);
    public string SellPriceMinDateString => Utilities.MarketPriceDateToString(SellPriceMinDate);
    public string SellPriceMinDateLastUpdateTime => Formatting.DateTimeToLastUpdateTime(SellPriceMinDate);
    public ulong SellPriceMax { get; set; }
    public string SellPriceMaxString => Utilities.UlongMarketPriceToString(SellPriceMax);
    public DateTime SellPriceMaxDate { get; set; }
    public ValueTimeStatus SellPriceMaxDateStatus => GetValueTimeStatus(SellPriceMaxDate);
    public string SellPriceMaxDateString => Utilities.MarketPriceDateToString(SellPriceMaxDate);
    public string SellPriceMaxDateLastUpdateTime => Formatting.DateTimeToLastUpdateTime(SellPriceMaxDate);
    public ulong BuyPriceMin { get; set; }
    public string BuyPriceMinString => Utilities.UlongMarketPriceToString(BuyPriceMin);
    public DateTime BuyPriceMinDate { get; set; }
    public ValueTimeStatus BuyPriceMinDateStatus => GetValueTimeStatus(BuyPriceMinDate);
    public string BuyPriceMinDateString => Utilities.MarketPriceDateToString(BuyPriceMinDate);
    public string BuyPriceMinDateLastUpdateTime => Formatting.DateTimeToLastUpdateTime(BuyPriceMinDate);
    public ulong BuyPriceMax { get; set; }
    public string BuyPriceMaxString => Utilities.UlongMarketPriceToString(BuyPriceMax);
    public DateTime BuyPriceMaxDate { get; set; }
    public ValueTimeStatus BuyPriceMaxDateStatus => GetValueTimeStatus(BuyPriceMaxDate);
    public string BuyPriceMaxDateString => Utilities.MarketPriceDateToString(BuyPriceMaxDate);
    public string BuyPriceMaxDateLastUpdateTime => Formatting.DateTimeToLastUpdateTime(BuyPriceMaxDate);
    public bool BestSellMinPrice { get; set; }
    public bool BestSellMaxPrice { get; set; }
    public bool BestBuyMinPrice { get; set; }
    public bool BestBuyMaxPrice { get; set; }

    private static ValueTimeStatus GetValueTimeStatus(DateTime dateTime)
    {
        if (dateTime.Date <= DateTime.MinValue.Date)
        {
            return ValueTimeStatus.NoValue;
        }

        if (dateTime.AddHours(1) < DateTime.UtcNow)
        {
            return ValueTimeStatus.ToOldFirst;
        }

        if (dateTime.AddHours(3) < DateTime.UtcNow)
        {
            return ValueTimeStatus.ToOldSecond;
        }

        if (dateTime.AddHours(9) < DateTime.UtcNow)
        {
            return ValueTimeStatus.ToOldThird;
        }

        return ValueTimeStatus.Normal;
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