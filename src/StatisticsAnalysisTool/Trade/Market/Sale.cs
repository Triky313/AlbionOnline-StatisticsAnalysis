namespace StatisticsAnalysisTool.Trade.Market;

public class Sale
{
    public long ObjectId { get; init; }
    public long AuctionId { get; init; }
    public int Amount { get; init; }
    public int ItemId { get; init; }

    public bool IsValid => AuctionId > 0 && Amount > 0 && ItemId > 0;
}