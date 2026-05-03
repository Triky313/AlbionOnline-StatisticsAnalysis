using System.Collections.Generic;

namespace StatisticsAnalysisTool.Trade.PlayerTrades;

public sealed class PlayerTradeUpdate
{
    public long TradeId { get; init; }
    public long Revision { get; init; }
    public long LocalSilverInternal { get; init; }
    public long PartnerSilverInternal { get; init; }
    public IReadOnlyList<PlayerTradeItem> LocalItems { get; init; } = [];
    public IReadOnlyList<PlayerTradeItem> PartnerItems { get; init; } = [];
}