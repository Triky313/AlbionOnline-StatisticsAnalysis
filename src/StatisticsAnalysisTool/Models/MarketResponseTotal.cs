using System;
using StatisticsAnalysisTool.Common;

namespace StatisticsAnalysisTool.Models;

public class MarketResponseTotal
{
    public MarketResponseTotal(MarketResponse item)
    {
        ItemTypeId = item.ItemTypeId;
        City = Locations.GetLocationByLocationNameOrId(item.City);
        QualityLevel = (byte) item.QualityLevel;
        SellPriceMin = item.SellPriceMin;
        SellPriceMax = item.SellPriceMax;
        BuyPriceMin = item.BuyPriceMin;
        BuyPriceMax = item.BuyPriceMax;
        SellPriceMinDate = item.SellPriceMinDate;
        SellPriceMaxDate = item.SellPriceMaxDate;
        BuyPriceMinDate = item.BuyPriceMinDate;
        BuyPriceMaxDate = item.BuyPriceMaxDate;
    }

    public string ItemTypeId { get; set; }
    public Location City { get; set; }
    public byte QualityLevel { get; set; }
    public ulong SellPriceMin { get; set; }
    public DateTime SellPriceMinDate { get; set; }
    public ulong SellPriceMax { get; set; }
    public DateTime SellPriceMaxDate { get; set; }
    public ulong BuyPriceMin { get; set; }
    public DateTime BuyPriceMinDate { get; set; }
    public ulong BuyPriceMax { get; set; }
    public DateTime BuyPriceMaxDate { get; set; }
    public bool BestSellMinPrice { get; set; }
    public bool BestSellMaxPrice { get; set; }
    public bool BestBuyMinPrice { get; set; }
    public bool BestBuyMaxPrice { get; set; }
}