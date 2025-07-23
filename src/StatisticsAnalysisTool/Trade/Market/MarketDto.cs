using System;

namespace StatisticsAnalysisTool.Trade.Market;

public class MarketDto
{
    public string UniqueName { get; set; }
    public string City { get; set; }
    public int QualityLevel { get; set; }
    public ulong SellPriceMin { get; set; }
    public DateTime SellPriceMinDate { get; set; }
    public ulong SellPriceMax { get; set; }
    public DateTime SellPriceMaxDate { get; set; }
    public ulong BuyPriceMin { get; set; }
    public DateTime BuyPriceMinDate { get; set; }
    public ulong BuyPriceMax { get; set; }
    public DateTime BuyPriceMaxDate { get; set; }
}