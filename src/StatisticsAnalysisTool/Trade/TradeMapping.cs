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
                MailContent = trade.MailContent,
                Description = trade.Description
            },
            TradeType.InstantBuy => new TradeDto()
            {
                Type = TradeType.InstantBuy,
                Id = trade.Id,
                Ticks = trade.Ticks,
                ClusterIndex = trade.ClusterIndex,
                InstantBuySellContent = trade.InstantBuySellContent,
                AuctionEntry = trade.AuctionEntry,
                Description = trade.Description,
                ItemIndex = trade.ItemIndex
            },
            TradeType.InstantSell => new TradeDto()
            {
                Type = TradeType.InstantSell,
                Id = trade.Id,
                Ticks = trade.Ticks,
                ClusterIndex = trade.ClusterIndex,
                InstantBuySellContent = trade.InstantBuySellContent,
                AuctionEntry = trade.AuctionEntry,
                Description = trade.Description
            },
            TradeType.ManualBuy => new TradeDto()
            {
                Type = TradeType.ManualBuy,
                Id = trade.Id,
                Ticks = trade.Ticks,
                InstantBuySellContent = trade.InstantBuySellContent,
                Description = trade.Description
            },
            TradeType.ManualSell => new TradeDto()
            {
                Type = TradeType.ManualSell,
                Id = trade.Id,
                Ticks = trade.Ticks,
                InstantBuySellContent = trade.InstantBuySellContent,
                Description = trade.Description
            },
            TradeType.Crafting => new TradeDto()
            {
                Type = TradeType.Crafting,
                Id = trade.Id,
                Ticks = trade.Ticks,
                InstantBuySellContent = trade.InstantBuySellContent,
                Description = trade.Description,
                ItemIndex = trade.ItemIndex
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
                MailContent = trade.MailContent ?? new MailContent(),
                Description = trade.Description
            },
            TradeType.InstantBuy => new Trade()
            {
                Id = trade.Id,
                Type = trade.Type,
                Ticks = trade.Ticks,
                ClusterIndex = trade.ClusterIndex,
                AuctionEntry = trade.AuctionEntry,
                InstantBuySellContent = trade.InstantBuySellContent ?? new InstantBuySellContent(),
                Description = trade.Description,
                ItemIndex = trade.ItemIndex
            },
            TradeType.InstantSell => new Trade()
            {
                Id = trade.Id,
                Type = trade.Type,
                Ticks = trade.Ticks,
                ClusterIndex = trade.ClusterIndex,
                AuctionEntry = trade.AuctionEntry,
                InstantBuySellContent = trade.InstantBuySellContent ?? new InstantBuySellContent(),
                Description = trade.Description
            },
            TradeType.ManualBuy => new Trade()
            {
                Type = TradeType.ManualBuy,
                Id = trade.Id,
                Ticks = trade.Ticks,
                InstantBuySellContent = trade.InstantBuySellContent ?? new InstantBuySellContent(),
                Description = trade.Description
            },
            TradeType.ManualSell => new Trade()
            {
                Type = TradeType.ManualSell,
                Id = trade.Id,
                Ticks = trade.Ticks,
                InstantBuySellContent = trade.InstantBuySellContent ?? new InstantBuySellContent(),
                Description = trade.Description
            },
            TradeType.Crafting => new Trade()
            {
                Type = TradeType.Crafting,
                Id = trade.Id,
                Ticks = trade.Ticks,
                InstantBuySellContent = trade.InstantBuySellContent ?? new InstantBuySellContent(),
                Description = trade.Description,
                ItemIndex = trade.ItemIndex
            },
            TradeType.Unknown => null,
            _ => null
        };
    }
}