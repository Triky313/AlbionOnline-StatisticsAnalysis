using StatisticsAnalysisTool.Common;

namespace StatisticsAnalysisTool.Trade;

public static class TradeMarketLocationResolver
{
    public static MarketLocation Resolve(MarketLocation location)
    {
        return location switch
        {
            MarketLocation.ThetfordPortal => MarketLocation.ThetfordMarket,
            MarketLocation.LymhurstPortal => MarketLocation.LymhurstMarket,
            MarketLocation.BridgewatchPortal => MarketLocation.BridgewatchMarket,
            MarketLocation.MartlockPortal => MarketLocation.MartlockMarket,
            MarketLocation.FortSterlingPortal => MarketLocation.FortSterlingMarket,
            _ => location
        };
    }
}