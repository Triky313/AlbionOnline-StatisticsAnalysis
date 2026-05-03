namespace StatisticsAnalysisTool.Trade.PlayerTrades;

public sealed class PlayerTradeSession(long tradeId, string partnerName)
{
    public long TradeId { get; } = tradeId;
    public string PartnerName { get; set; } = partnerName ?? string.Empty;
    public PlayerTradeUpdate LastUpdate { get; set; }
}