using Albion.Network;
using log4net;
using Newtonsoft.Json;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Models.NetworkModel;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StatisticsAnalysisTool.Network.Operations.Responses
{
    public class AuctionGetOffersOperation : BaseOperation
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public List<MarketOffer> MarketOffers { get; set; } = new List<MarketOffer>();

        public AuctionGetOffersOperation(Dictionary<byte, object> parameters) : base(parameters)
        {
            ConsoleManager.WriteLineForNetworkHandler(GetType().Name, parameters);

            try
            {
                if (parameters.ContainsKey(0))
                {
                    var _tempList = parameters[0].ObjectToList();
                    MarketOffers = _tempList.ConvertAll(x => ConvertMarketOfferStructToMarketOffer(JsonConvert.DeserializeObject<MarketOfferStruct>(x)));
                }
            }
            catch (Exception e)
            {
                Log.Debug(nameof(PartyMakeLeaderResponse), e);
            }
        }

        private MarketOffer ConvertMarketOfferStructToMarketOffer(MarketOfferStruct marketOfferStruct)
        {
            return new MarketOffer()
            {
                Id = marketOfferStruct.Id,
                UnitPriceSilver = FixPoint.FromInternalValue(marketOfferStruct.UnitPriceSilver),
                TotalPriceSilver = FixPoint.FromInternalValue(marketOfferStruct.TotalPriceSilver),
                Amount = marketOfferStruct.Amount,
                Tier = marketOfferStruct.Tier,
                IsFinished = marketOfferStruct.IsFinished,
                AuctionType = marketOfferStruct.AuctionType,
                HasBuyerFetched = marketOfferStruct.HasBuyerFetched,
                HasSellerFetched = marketOfferStruct.HasSellerFetched,
                SellerCharacterId = marketOfferStruct.SellerCharacterId,
                SellerName = marketOfferStruct.SellerName,
                BuyerCharacterId = marketOfferStruct.BuyerCharacterId,
                BuyerName = marketOfferStruct.BuyerName,
                ItemTypeId = marketOfferStruct.ItemTypeId,
                ItemGroupTypeId = marketOfferStruct.ItemGroupTypeId,
                EnchantmentLevel = marketOfferStruct.EnchantmentLevel,
                QualityLevel = marketOfferStruct.QualityLevel,
                Expires = marketOfferStruct.Expires,
                ReferenceId = marketOfferStruct.ReferenceId
            };
        }

        public struct MarketOfferStruct
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
}