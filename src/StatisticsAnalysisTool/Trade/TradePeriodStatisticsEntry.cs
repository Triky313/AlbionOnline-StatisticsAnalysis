namespace StatisticsAnalysisTool.Trade;

public sealed class TradePeriodStatisticsEntry
{
    public TradePeriodStatisticsEntry(string label, long sold, long bought, long tax, long netProfit, bool isTotal = false)
    {
        Label = label;
        Sold = sold;
        Bought = bought;
        Tax = tax;
        NetProfit = netProfit;
        IsTotal = isTotal;
    }

    public string Label { get; }
    public long Sold { get; }
    public long Bought { get; }
    public long Tax { get; }
    public long NetProfit { get; }
    public bool IsTotal { get; }
    public bool IsNetProfitNegative => NetProfit < 0;
}