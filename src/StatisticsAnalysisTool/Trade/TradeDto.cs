using StatisticsAnalysisTool.Trade.Mails;
using StatisticsAnalysisTool.Trade.Market;
using System;

namespace StatisticsAnalysisTool.Trade;

public class TradeDto
{
    public long Id { get; init; }
    public long Ticks { get; init; }
    public string ClusterIndex { get; init; }
    public TradeType Type { get; init; }
    public double TaxRate { get; set; }

    #region Mail

    public Guid Guid { get; init; }
    public string MailTypeText { get; init; }
    public MailContent MailContent { get; init; }

    #endregion

    #region Instant sell/buy

    public int Amount { get; init; }
    public AuctionEntry AuctionEntry { get; init; }
    public InstantBuySellContent InstantBuySellContent { get; init; }
    #endregion
}