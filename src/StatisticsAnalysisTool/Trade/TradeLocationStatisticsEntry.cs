using StatisticsAnalysisTool.Common;
using System.Windows.Media;

namespace StatisticsAnalysisTool.Trade;

public sealed class TradeLocationStatisticsEntry
{
    public MarketLocation Location { get; init; }

    public string LocationName { get; init; } = string.Empty;

    public Color LocationColor => Locations.GetLocationColor(Location);

    public int SalesCount { get; init; }

    public double SalesValue { get; init; }

    public int PurchasesCount { get; init; }

    public double PurchasesValue { get; init; }

    public double NetProfit { get; init; }

    public double Margin { get; init; }

    public double TaxPaid { get; init; }

    public string MostTradedCategory { get; init; } = string.Empty;

    public bool IsNetProfitNegative => NetProfit < 0d;

    public bool IsMarginNegative => Margin < 0d;
}