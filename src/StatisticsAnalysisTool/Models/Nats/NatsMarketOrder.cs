using StatisticsAnalysisTool.Models.NetworkModel;
using System.Globalization;
using System.Text.Json.Serialization;
using StatisticsAnalysisTool.Common;

namespace StatisticsAnalysisTool.Models.Nats;

public class NatsMarketOrder
{
    public NatsMarketOrder(AuctionGetOffer auctionGetOffer, int locationId = 0)
    {
        Id = (int)auctionGetOffer.Id;
        ItemId = auctionGetOffer.ItemTypeId;
        GroupTypeId = auctionGetOffer.ItemGroupTypeId;
        LocationId = locationId;
        QualityLevel = auctionGetOffer.QualityLevel;
        EnchantmentLevel = auctionGetOffer.EnchantmentLevel;
        Price = (int) FixPoint.FromInternalValue(auctionGetOffer.UnitPriceSilver).IntegerValue;
        Amount = auctionGetOffer.Amount;
        AuctionType = auctionGetOffer.AuctionType;
        Expires = auctionGetOffer.Expires.ToString("O");

    }

    public int Id { get; set; }
    [JsonPropertyName("ItemTypeId")]
    public string ItemId { get; set; }
    [JsonPropertyName("ItemGroupTypeId")]
    public string GroupTypeId { get; set; }
    [JsonPropertyName("LocationId")]
    public int LocationId { get; set; }
    [JsonPropertyName("QualityLevel")]
    public int QualityLevel { get; set; }
    [JsonPropertyName("EnchantmentLevel")]
    public int EnchantmentLevel { get; set; }
    [JsonPropertyName("UnitPriceSilver")]
    public int Price { get; set; }
    [JsonPropertyName("Amount")]
    public int Amount { get; set; }
    [JsonPropertyName("AuctionType")]
    public string AuctionType { get; set; }
    [JsonPropertyName("Expires")]
    public string Expires { get; set; }
}