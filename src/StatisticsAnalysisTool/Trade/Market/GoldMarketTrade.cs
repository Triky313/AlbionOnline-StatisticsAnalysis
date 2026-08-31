namespace StatisticsAnalysisTool.Trade.Market;

public sealed class GoldMarketTrade
{
    public const string ItemTypeId = "GOLD";

    public int Quantity { get; init; }
    public long InternalTotalPrice { get; init; }
    public long InternalUnitPrice => Quantity > 0 ? InternalTotalPrice / Quantity : 0;
    public bool IsValid => Quantity > 0 && InternalTotalPrice > 0;
}