using StatisticsAnalysisTool.Trade.Mails;
using StatisticsAnalysisTool.Trade.Market;

namespace StatisticsAnalysisTool.Trade;

public static class TradeMapping
{
    public static TradeDto Mapping(Trade trade)
    {
        return trade switch
        {
            Mail mail => new TradeDto()
            {
                Type = TradeType.Mail,
                Id = mail.Id,
                Ticks = mail.Ticks,
                ClusterIndex = mail.ClusterIndex,
                MailTypeText = mail.MailTypeText,
                Guid = mail.Guid,
                MailContent = mail.MailContent
            },
            InstantBuy instantBuy => new TradeDto()
            {
                Type = TradeType.InstantBuy,
                Id = instantBuy.Id,
                Ticks = instantBuy.Ticks,
                ClusterIndex = instantBuy.ClusterIndex,
                Amount = instantBuy.Amount,
                AuctionEntry = instantBuy.AuctionEntry
            },
            InstantSell instantSell => new TradeDto()
            {
                Type = TradeType.InstantBuy,
                Id = instantSell.Id,
                Ticks = instantSell.Ticks,
                ClusterIndex = instantSell.ClusterIndex,
                Amount = instantSell.Amount,
                AuctionEntry = instantSell.AuctionEntry
            },
            _ => new TradeDto()
        };
    }

    public static Trade Mapping(TradeDto trade)
    {
        return trade.Type switch
        {
            TradeType.Mail => new Mail()
            {
                Id = trade.Id,
                Ticks = trade.Ticks,
                ClusterIndex = trade.ClusterIndex,
                MailTypeText = trade.MailTypeText,
                Guid = trade.Guid,
                MailContent = trade.MailContent
            },
            TradeType.InstantBuy => new InstantBuy()
            {
                Id = trade.Id,
                Ticks = trade.Ticks,
                ClusterIndex = trade.ClusterIndex,
                Amount = trade.Amount,
                AuctionEntry = trade.AuctionEntry
            },
            TradeType.InstantSell => new InstantSell()
            {
                Id = trade.Id,
                Ticks = trade.Ticks,
                ClusterIndex = trade.ClusterIndex,
                Amount = trade.Amount,
                AuctionEntry = trade.AuctionEntry
            },
            TradeType.Unknown => null,
            _ => null
        };
    }
}