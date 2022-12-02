using System;
using System.Text.Json.Serialization;
using StatisticsAnalysisTool.Common;

namespace StatisticsAnalysisTool.Models;

public class MarketResponse
{
    [JsonPropertyName("item_id")]
    public string ItemTypeId { get; set; }

    [JsonPropertyName("city")]
    public string City { get; set; }

    public MarketLocation MarketLocation => City.GetMarketLocationByLocationNameOrId();

    [JsonPropertyName("quality")]
    public int QualityLevel { get; set; }

    [JsonPropertyName("sell_price_min")]
    public ulong SellPriceMin { get; set; }

    [JsonPropertyName("sell_price_min_date")]
    public DateTime SellPriceMinDate { get; set; }

    [JsonPropertyName("sell_price_max")]
    public ulong SellPriceMax { get; set; }

    [JsonPropertyName("sell_price_max_date")]
    public DateTime SellPriceMaxDate { get; set; }

    [JsonPropertyName("buy_price_min")]
    public ulong BuyPriceMin { get; set; }

    [JsonPropertyName("buy_price_min_date")]
    public DateTime BuyPriceMinDate { get; set; }

    [JsonPropertyName("buy_price_max")]
    public ulong BuyPriceMax { get; set; }

    [JsonPropertyName("buy_price_max_date")]
    public DateTime BuyPriceMaxDate { get; set; }
}