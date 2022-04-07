using StatisticsAnalysisTool.Common;
using System;
using System.Collections.Generic;
using System.Windows;

namespace StatisticsAnalysisTool.Models
{
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