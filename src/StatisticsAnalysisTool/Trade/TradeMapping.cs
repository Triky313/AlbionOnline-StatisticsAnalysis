using StatisticsAnalysisTool.Trade.Mails;
using StatisticsAnalysisTool.Trade.Market;
using System;

namespace StatisticsAnalysisTool.Trade;

public static class TradeMapping
{
    public static TradeDto Mapping(Trade trade)
    {
        return trade.Type switch
        {
            TradeType.Mail => new TradeDto()
            {
                Type = TradeType.Mail,
                Id = trade.Id,
                Ticks = trade.Ticks,
                ClusterIndex = trade.ClusterIndex,
                MailTypeText = trade.MailTypeText,
                Guid = trade.Guid,
                MailContent = trade.MailContent
            },
            TradeType.InstantBuy => new TradeDto()
            {
                Type = TradeType.InstantBuy,
                Id = trade.Id,
                Ticks = trade.Ticks,
                ClusterIndex = trade.ClusterIndex,
                InstantBuySellContent = trade.InstantBuySellContent,
                AuctionEntry = trade.AuctionEntry
            },
            TradeType.InstantSell => new TradeDto()
            {
                Type = TradeType.InstantSell,
                Id = trade.Id,
                Ticks = trade.Ticks,
                ClusterIndex = trade.ClusterIndex,
                InstantBuySellContent = trade.InstantBuySellContent,
                AuctionEntry = trade.AuctionEntry
            },
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public static Trade Mapping(TradeDto trade)
    {
        return trade.Type switch
        {
            TradeType.Mail => new Trade()
            {
                Id = trade.Id,
                Type = trade.Type,
                Ticks = trade.Ticks,
                ClusterIndex = trade.ClusterIndex,
                MailTypeText = trade.MailTypeText,
                Guid = trade.Guid,
                MailContent = trade.MailContent ?? new MailContent()
            },
            TradeType.InstantBuy => new Trade()
            {
                Id = trade.Id,
                Type = trade.Type,
                Ticks = trade.Ticks,
                ClusterIndex = trade.ClusterIndex,
                AuctionEntry = trade.AuctionEntry,
                InstantBuySellContent = trade.InstantBuySellContent ?? new InstantBuySellContent()
            },
            TradeType.InstantSell => new Trade()
            {
                Id = trade.Id,
                Type = trade.Type,
                Ticks = trade.Ticks,
                ClusterIndex = trade.ClusterIndex,
                AuctionEntry = trade.AuctionEntry,
                InstantBuySellContent = trade.InstantBuySellContent ?? new InstantBuySellContent()
            },
            TradeType.Unknown => null,
            _ => null
        };
    }
}