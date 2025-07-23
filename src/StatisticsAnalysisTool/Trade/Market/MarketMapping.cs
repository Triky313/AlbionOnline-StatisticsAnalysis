using StatisticsAnalysisTool.Models;

namespace StatisticsAnalysisTool.Trade.Market;

public static class MarketMapping
{
    public static MarketDto Mapping(MarketResponse marketResponse)
    {

        return new MarketDto()
        {
            UniqueName = marketResponse.ItemTypeId,
            City = marketResponse.City,
            QualityLevel = marketResponse.QualityLevel,
            SellPriceMin = marketResponse.SellPriceMin,
            SellPriceMinDate = marketResponse.SellPriceMinDate,
            SellPriceMax = marketResponse.SellPriceMax,
            SellPriceMaxDate = marketResponse.SellPriceMaxDate,
            BuyPriceMin = marketResponse.BuyPriceMin,
            BuyPriceMinDate = marketResponse.BuyPriceMinDate,
            BuyPriceMax = marketResponse.BuyPriceMax,
            BuyPriceMaxDate = marketResponse.BuyPriceMaxDate
        };
    }

    public static MarketResponse Mapping(MarketDto marketResponse)
    {
        return new MarketResponse()
        {
            ItemTypeId = marketResponse.UniqueName,
            City = marketResponse.City,
            QualityLevel = marketResponse.QualityLevel,
            SellPriceMin = marketResponse.SellPriceMin,
            SellPriceMinDate = marketResponse.SellPriceMinDate,
            SellPriceMax = marketResponse.SellPriceMax,
            SellPriceMaxDate = marketResponse.SellPriceMaxDate,
            BuyPriceMin = marketResponse.BuyPriceMin,
            BuyPriceMinDate = marketResponse.BuyPriceMinDate,
            BuyPriceMax = marketResponse.BuyPriceMax,
            BuyPriceMaxDate = marketResponse.BuyPriceMaxDate
        };
    }
}