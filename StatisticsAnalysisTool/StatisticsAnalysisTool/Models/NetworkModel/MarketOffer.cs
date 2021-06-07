using StatisticsAnalysisTool.Common;
using System;

namespace StatisticsAnalysisTool.Models.NetworkModel
{
    public class MarketOffer
    {
        public MarketOffer()
        {
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public DateTime CreatedAt { get; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public long Id { get; set; }
        public FixPoint UnitPriceSilver { get; set; }
        public FixPoint TotalPriceSilver { get; set; }
        public int Amount { get; set; }
        public short Tier { get; set; }
        public bool IsFinished { get; set; }
        public string AuctionType { get; set; }
        public bool HasBuyerFetched { get; set; }
        public bool HasSellerFetched { get; set; }
        public string SellerCharacterId { get; set; }
        public string SellerName { get; set; }
        public string BuyerCharacterId { get; set; }
        public string BuyerName { get; set; }
        public string ItemTypeId { get; set; }
        public string ItemGroupTypeId { get; set; }
        public short EnchantmentLevel { get; set; }
        public short QualityLevel { get; set; }
        public string Expires { get; set; }
        public string ReferenceId { get; set; }
    }
}