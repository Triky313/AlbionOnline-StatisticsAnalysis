using System;
using System.Windows;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.GameData;

namespace StatisticsAnalysisTool.Models;

public class MarketCurrentPricesItem
{
    public MarketCurrentPricesItem(MarketResponseTotal marketResponseTotal)
    {
        ItemTypeId = marketResponseTotal.ItemTypeId;
        Location = marketResponseTotal.City;
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
    public Location Location { get; set; }
    public string LocationName => WorldData.GetUniqueNameOrDefault((int)Location);
    public byte QualityLevel { get; set; }
    public ulong SellPriceMin { get; set; }
    public string SellPriceMinString => Utilities.UlongMarketPriceToString(SellPriceMin);
    public DateTime SellPriceMinDate { get; set; }
    public string SellPriceMinDateString => Utilities.MarketPriceDateToString(SellPriceMinDate);
    public string SellPriceMinDateLastUpdateTime => Formatting.DateTimeToLastUpdateTime(SellPriceMinDate);
    public ulong SellPriceMax { get; set; }
    public string SellPriceMaxString => Utilities.UlongMarketPriceToString(SellPriceMax);
    public DateTime SellPriceMaxDate { get; set; }
    public string SellPriceMaxDateString => Utilities.MarketPriceDateToString(SellPriceMaxDate);
    public string SellPriceMaxDateLastUpdateTime => Formatting.DateTimeToLastUpdateTime(SellPriceMaxDate);
    public ulong BuyPriceMin { get; set; }
    public string BuyPriceMinString => Utilities.UlongMarketPriceToString(BuyPriceMin);
    public DateTime BuyPriceMinDate { get; set; }
    public string BuyPriceMinDateString => Utilities.MarketPriceDateToString(BuyPriceMinDate);
    public string BuyPriceMinDateLastUpdateTime => Formatting.DateTimeToLastUpdateTime(BuyPriceMinDate);
    public ulong BuyPriceMax { get; set; }
    public string BuyPriceMaxString => Utilities.UlongMarketPriceToString(BuyPriceMax);
    public DateTime BuyPriceMaxDate { get; set; }
    public string BuyPriceMaxDateString => Utilities.MarketPriceDateToString(BuyPriceMaxDate);
    public string BuyPriceMaxDateLastUpdateTime => Formatting.DateTimeToLastUpdateTime(BuyPriceMaxDate);
    public bool BestSellMinPrice { get; set; }
    public bool BestSellMaxPrice { get; set; }
    public bool BestBuyMinPrice { get; set; }
    public bool BestBuyMaxPrice { get; set; }

    public Style LocationStyle => ItemController.LocationStyle(Location);

    public Style SellPriceMinStyle => ItemController.PriceStyle(BestSellMinPrice);

    public Style BuyPriceMaxStyle => ItemController.PriceStyle(BestBuyMaxPrice);

    public Style SellPriceMinDateStyle => ItemController.GetStyleByTimestamp(SellPriceMinDate);

    public Style SellPriceMaxDateStyle => ItemController.GetStyleByTimestamp(SellPriceMaxDate);

    public Style BuyPriceMinDateStyle => ItemController.GetStyleByTimestamp(BuyPriceMinDate);

    public Style BuyPriceMaxDateStyle => ItemController.GetStyleByTimestamp(BuyPriceMaxDate);
}