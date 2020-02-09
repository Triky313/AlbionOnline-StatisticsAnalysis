using Newtonsoft.Json;
using StatisticsAnalysisTool.Common;
using System;
using System.Globalization;
using System.Windows;

namespace StatisticsAnalysisTool.Models
{
    //public class MarketOrder
    //{
    //    public ulong Id { get; set; }
    //    public string ItemTypeId { get; set; }
    //    public ushort LocationId { get; set; }
    //    public byte QualityLevel { get; set; }
    //    public byte EnchantmentLevel { get; set; }
    //    public ulong UnitPriceSilver { get; set; }
    //    public uint Amount { get; set; }
    //    public string AuctionType { get; set; }
    //    public DateTime Expires { get; set; }

    //    [NotMapped]
    //    public string ItemGroupTypeId { get; set; }

    //    public override string ToString()
    //    {
    //        return $"{Id}{Amount}";
    //    }
    //}

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
        public string SellPriceMinString => SellPriceMin.ToString("N0", new CultureInfo(LanguageController.CurrentLanguage));
        public DateTime SellPriceMinDate { get; set; }
        public string SellPriceMinDateString => DateTime.SpecifyKind(SellPriceMinDate, DateTimeKind.Utc).ToLocalTime().ToString("G", new CultureInfo(LanguageController.CurrentLanguage));
        public ulong SellPriceMax { get; set; }
        public string SellPriceMaxString => SellPriceMax.ToString("N0", new CultureInfo(LanguageController.CurrentLanguage));
        public DateTime SellPriceMaxDate { get; set; }
        public string SellPriceMaxDateString => DateTime.SpecifyKind(SellPriceMaxDate, DateTimeKind.Utc).ToLocalTime().ToString("G", new CultureInfo(LanguageController.CurrentLanguage));
        public ulong BuyPriceMin { get; set; }
        public string BuyPriceMinString => BuyPriceMin.ToString("N0", new CultureInfo(LanguageController.CurrentLanguage));
        public DateTime BuyPriceMinDate { get; set; }
        public string BuyPriceMinDateString => DateTime.SpecifyKind(BuyPriceMinDate, DateTimeKind.Utc).ToLocalTime().ToString("G", new CultureInfo(LanguageController.CurrentLanguage));
        public ulong BuyPriceMax { get; set; }
        public string BuyPriceMaxString => BuyPriceMax.ToString("N0", new CultureInfo(LanguageController.CurrentLanguage));
        public DateTime BuyPriceMaxDate { get; set; }
        public string BuyPriceMaxDateString => DateTime.SpecifyKind(BuyPriceMaxDate, DateTimeKind.Utc).ToLocalTime().ToString("G", new CultureInfo(LanguageController.CurrentLanguage));
        public bool BestSellMinPrice { get; set; }
        public bool BestSellMaxPrice { get; set; }
        public bool BestBuyMinPrice { get; set; }
        public bool BestBuyMaxPrice { get; set; }

        public Style LocationStyle
        {
            get {
                switch (Location)
                {
                    case Location.Caerleon:
                        return Application.Current.FindResource("CaerleonStyle") as Style;
                    case Location.Thetford:
                        return Application.Current.FindResource("ThetfordStyle") as Style;
                    case Location.Bridgewatch:
                        return Application.Current.FindResource("BridgewatchStyle") as Style;
                    case Location.Martlock:
                        return Application.Current.FindResource("MartlockStyle") as Style;
                    case Location.Lymhurst:
                        return Application.Current.FindResource("LymhurstStyle") as Style;
                    case Location.FortSterling:
                        return Application.Current.FindResource("FortSterlingStyle") as Style;
                    case Location.ArthursRest:
                        return Application.Current.FindResource("ArthursRestStyle") as Style;
                    case Location.MerlynsRest:
                        return Application.Current.FindResource("MerlynsRestStyle") as Style;
                    case Location.MorganasRest:
                        return Application.Current.FindResource("MorganasRestStyle") as Style;
                    default:
                        return Application.Current.FindResource("DefaultCityStyle") as Style;
                }
            }
        }

        public Style SellPriceMinStyle
        {
            get
            {
                switch (BestSellMinPrice)
                {
                    case true:
                        return Application.Current.FindResource("ListView.Grid.StackPanel.Label.BestPrice") as Style;
                    case false:
                        return Application.Current.FindResource("ListView.Grid.StackPanel.Label.Price") as Style;
                    default:
                        return Application.Current.FindResource("ListView.Grid.StackPanel.Label.Price") as Style;
                }

            }
        }

        public Style BuyPriceMaxStyle {
            get 
            {
                switch (BestBuyMaxPrice)
                {
                    case true:
                        return Application.Current.FindResource("ListView.Grid.StackPanel.Label.BestPrice") as Style;
                    case false:
                        return Application.Current.FindResource("ListView.Grid.StackPanel.Label.Price") as Style;
                    default:
                        return Application.Current.FindResource("ListView.Grid.StackPanel.Label.Price") as Style;
                }

            }
        }

        private Style GetStyleByTimestamp(DateTime value)
        {
            if (value.Date == DateTime.MinValue.Date)
                return Application.Current.FindResource("ListView.Grid.Label.Date.NoValue") as Style;

            if (value.AddHours(8) < DateTime.Now.ToUniversalTime().AddHours(-1))
                return Application.Current.FindResource("ListView.Grid.Label.Date.ToOldFirst") as Style;

            if (value.AddHours(4) < DateTime.Now.ToUniversalTime().AddHours(-1))
                return Application.Current.FindResource("ListView.Grid.Label.Date.ToOldSecond") as Style;

            if (value.AddHours(2) < DateTime.Now.ToUniversalTime().AddHours(-1))
                return Application.Current.FindResource("ListView.Grid.Label.Date.ToOldThird") as Style;

            return Application.Current.FindResource("ListView.Grid.Label.Date.Normal") as Style;
        }

        public Style SellPriceMinDateStyle => GetStyleByTimestamp(SellPriceMinDate);

        public Style SellPriceMaxDateStyle => GetStyleByTimestamp(SellPriceMaxDate);

        public Style BuyPriceMinDateStyle => GetStyleByTimestamp(BuyPriceMinDate);

        public Style BuyPriceMaxDateStyle => GetStyleByTimestamp(BuyPriceMaxDate);

    }
}