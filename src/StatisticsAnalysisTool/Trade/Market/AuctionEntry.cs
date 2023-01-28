using System;
using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Trade.Market;

public class AuctionEntry
{
    public long Id { get; set; }
    public int UnitPriceSilver { get; set; }
    public int TotalPriceSilver { get; set; }
    public int Amount { get; set; }
    public int Tier { get; set; }
    public bool IsFinished { get; set; }
    public string AuctionType { get; set; }
    public bool HasBuyerFetched { get; set; }
    public bool HasSellerFetched { get; set; }
    public string SellerCharacterId { get; set; }
    public string SellerName { get; set; }
    public object BuyerCharacterId { get; set; }
    public object BuyerName { get; set; }
    public string ItemTypeId { get; set; }
    public string ItemGroupTypeId { get; set; }
    public int EnchantmentLevel { get; set; }
    public int QualityLevel { get; set; }
    public DateTime Expires { get; set; }
    public string ReferenceId { get; set; }
}