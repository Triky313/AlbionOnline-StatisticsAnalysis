namespace StatisticsAnalysisTool.Trade;

public readonly record struct TradeAnalyticsValueBreakdown(double Sold, double Bought, double Tax, long SoldQuantity, long BoughtQuantity)
{
    public static TradeAnalyticsValueBreakdown Empty => new(0d, 0d, 0d, 0, 0);

    public double NetProfit => Sold - Bought - Tax;

    public double InvestedCapital => Bought + Tax;
}