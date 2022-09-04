using System;
using System.Text.Json.Serialization;
using StatisticsAnalysisTool.Common.Converters;

namespace StatisticsAnalysisTool.Models.NetworkModel;

public class AuctionGetOffer
{
    public long Id { get; set; }
    public long UnitPriceSilver { get; set; }
    public long TotalPriceSilver { get; set; }
    public int Amount { get; set; }
    public short Tier { get; set; }
    public bool IsFinished { get; set; }
    public string AuctionType { get; set; }
    public bool HasBuyerFetched { get; set; }
    public bool HasSellerFetched { get; set; }
    [JsonConverter(typeof(GuidConverter))]
    public Guid SellerCharacterId { get; set; }
    public string SellerName { get; set; }
    [JsonConverter(typeof(GuidConverter))]
    public Guid BuyerCharacterId { get; set; }
    public string BuyerName { get; set; }
    public string ItemTypeId { get; set; }
    public string ItemGroupTypeId { get; set; }
    public short EnchantmentLevel { get; set; }
    public short QualityLevel { get; set; }
    public DateTime Expires { get; set; }
    [JsonConverter(typeof(GuidConverter))]
    public Guid ReferenceId { get; set; }
}