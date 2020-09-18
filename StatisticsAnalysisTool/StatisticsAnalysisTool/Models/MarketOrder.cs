using Newtonsoft.Json;
using StatisticsAnalysisTool.Common;
using System;
using System.Collections.Generic;
using System.Windows;

namespace StatisticsAnalysisTool.Models
{
    public class MarketResponse
    {
        [JsonProperty(PropertyName = "item_id")]
        public string ItemTypeId { get; set; }

        [JsonProperty(PropertyName = "city")]
        public string City { get; set; }

        [JsonProperty(PropertyName = "quality")]
        public int QualityLevel { get; set; }

        [JsonProperty(PropertyName = "sell_price_min")]
        public ulong SellPriceMin { get; set; }

        [JsonProperty(PropertyName = "sell_price_min_date")]
        public DateTime SellPriceMinDate { get; set; }

        [JsonProperty(PropertyName = "sell_price_max")]
        public ulong SellPriceMax { get; set; }

        [JsonProperty(PropertyName = "sell_price_max_date")]
        public DateTime SellPriceMaxDate { get; set; }

        [JsonProperty(PropertyName = "buy_price_min")]
        public ulong BuyPriceMin { get; set; }

        [JsonProperty(PropertyName = "buy_price_min_date")]
        public DateTime BuyPriceMinDate { get; set; }

        [JsonProperty(PropertyName = "buy_price_max")]
        public ulong BuyPriceMax { get; set; }

        [JsonProperty(PropertyName = "buy_price_max_date")]
        public DateTime BuyPriceMaxDate { get; set; }
    }

    public class MarketResponseTotal
    {
        public MarketResponseTotal(MarketResponse item)
        {
            ItemTypeId = item.ItemTypeId;
            City = Locations.GetName(item.City);
            QualityLevel = (byte)item.QualityLevel;
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
        public string LocationName => Locations.GetName(Location);
        public byte QualityLevel { get; set; }
        public ulong SellPriceMin { get; set; }
        public string SellPriceMinString => Utilities.UlongMarketPriceToString(SellPriceMin);
        public DateTime SellPriceMinDate { get; set; }
        public string SellPriceMinDateString => Utilities.MarketPriceDateToString(SellPriceMinDate);
        public ulong SellPriceMax { get; set; }
        public string SellPriceMaxString => Utilities.UlongMarketPriceToString(SellPriceMax);
        public DateTime SellPriceMaxDate { get; set; }
        public string SellPriceMaxDateString => Utilities.MarketPriceDateToString(SellPriceMaxDate);
        public ulong BuyPriceMin { get; set; }
        public string BuyPriceMinString => Utilities.UlongMarketPriceToString(BuyPriceMin);
        public DateTime BuyPriceMinDate { get; set; }
        public string BuyPriceMinDateString => Utilities.MarketPriceDateToString(BuyPriceMinDate);
        public ulong BuyPriceMax { get; set; }
        public string BuyPriceMaxString => Utilities.UlongMarketPriceToString(BuyPriceMax);
        public DateTime BuyPriceMaxDate { get; set; }
        public string BuyPriceMaxDateString => Utilities.MarketPriceDateToString(BuyPriceMaxDate);
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

    public class MarketQualityObject
    {
        public string Location { get; set; }
        public string LocationName => Locations.GetName(LocationType);
        private Location LocationType => Locations.GetName(Location);
        public ulong SellPriceMinNormal { private get; set; }
        public ulong SellPriceMinGood { private get; set; }
        public ulong SellPriceMinOutstanding { private get; set; }
        public ulong SellPriceMinExcellent { private get; set; }
        public ulong SellPriceMinMasterpiece { private get; set; }

        public string SellPriceMinNormalString => Utilities.UlongMarketPriceToString(SellPriceMinNormal);
        public string SellPriceMinGoodString => Utilities.UlongMarketPriceToString(SellPriceMinGood);
        public string SellPriceMinOutstandingString => Utilities.UlongMarketPriceToString(SellPriceMinOutstanding);
        public string SellPriceMinExcellentString => Utilities.UlongMarketPriceToString(SellPriceMinExcellent);
        public string SellPriceMinMasterpieceString => Utilities.UlongMarketPriceToString(SellPriceMinMasterpiece);

        public string SellPriceMinNormalStringInRalMoney { get; set; }
        public string SellPriceMinGoodStringInRalMoney { get; set; }
        public string SellPriceMinOutstandingStringInRalMoney { get; set; }
        public string SellPriceMinExcellentStringInRalMoney { get; set; }
        public string SellPriceMinMasterpieceStringInRalMoney { get; set; }

        public DateTime SellPriceMinNormalDate { private get; set; }
        public DateTime SellPriceMinGoodDate { private get; set; }
        public DateTime SellPriceMinOutstandingDate { private get; set; }
        public DateTime SellPriceMinExcellentDate { private get; set; }
        public DateTime SellPriceMinMasterpieceDate { private get; set; }

        public string SellPriceMinNormalDateString => Utilities.MarketPriceDateToString(SellPriceMinNormalDate);
        public string SellPriceMinGoodDateString => Utilities.MarketPriceDateToString(SellPriceMinGoodDate);
        public string SellPriceMinOutstandingDateString => Utilities.MarketPriceDateToString(SellPriceMinOutstandingDate);
        public string SellPriceMinExcellentDateString => Utilities.MarketPriceDateToString(SellPriceMinExcellentDate);
        public string SellPriceMinMasterpieceDateString => Utilities.MarketPriceDateToString(SellPriceMinMasterpieceDate);

        public Style LocationStyle => ItemController.LocationStyle(LocationType);

        public Style SellPriceMinNormalStyle => ItemController.PriceStyle(BestMinPrice() == SellPriceMinNormal);
        public Style SellPriceMinGoodStyle => ItemController.PriceStyle(BestMinPrice() == SellPriceMinGood);
        public Style SellPriceMinOutstandingStyle => ItemController.PriceStyle(BestMinPrice() == SellPriceMinOutstanding);
        public Style SellPriceMinExcellentStyle => ItemController.PriceStyle(BestMinPrice() == SellPriceMinExcellent);
        public Style SellPriceMinMasterpieceStyle => ItemController.PriceStyle(BestMinPrice() == SellPriceMinMasterpiece);

        private ulong BestMinPrice()
        {
            var priceList = new List<ulong>
            {
                SellPriceMinNormal,
                SellPriceMinGood,
                SellPriceMinOutstanding,
                SellPriceMinExcellent,
                SellPriceMinMasterpiece
            };

            return ItemController.GetMinPrice(priceList);
        }
    }
}