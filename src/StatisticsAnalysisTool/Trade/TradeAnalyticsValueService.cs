using StatisticsAnalysisTool.Enumerations;

namespace StatisticsAnalysisTool.Trade;

public sealed class TradeAnalyticsValueService
{
    public TradeAnalyticsValueBreakdown GetBreakdown(Trade trade)
    {
        if (trade == null || !IsRelevantForAnalytics(trade))
        {
            return TradeAnalyticsValueBreakdown.Empty;
        }

        return trade.Type switch
        {
            TradeType.Mail when trade.MailType is MailType.MarketplaceSellOrderFinished or MailType.MarketplaceSellOrderExpired => new TradeAnalyticsValueBreakdown(
                trade.MailContent.TotalPrice.IntegerValue,
                0d,
                trade.MailContent.TaxSetupPrice.IntegerValue + trade.MailContent.TaxPrice.IntegerValue,
                GetMailQuantity(trade),
                0),
            TradeType.Mail when trade.MailType is MailType.MarketplaceBuyOrderFinished or MailType.MarketplaceBuyOrderExpired => new TradeAnalyticsValueBreakdown(
                0d,
                trade.MailContent.TotalPrice.IntegerValue,
                trade.MailContent.TaxSetupPrice.IntegerValue + trade.MailContent.TaxPrice.IntegerValue,
                0,
                GetMailQuantity(trade)),
            TradeType.InstantSell => new TradeAnalyticsValueBreakdown(
                trade.InstantBuySellContent.TotalPrice.IntegerValue,
                0d,
                trade.InstantBuySellContent.TaxPrice.IntegerValue,
                trade.InstantBuySellContent.Quantity,
                0),
            TradeType.InstantBuy => new TradeAnalyticsValueBreakdown(
                0d,
                trade.InstantBuySellContent.TotalPrice.IntegerValue,
                0d,
                0,
                trade.InstantBuySellContent.Quantity),
            TradeType.ManualSell => new TradeAnalyticsValueBreakdown(
                trade.InstantBuySellContent.TotalPrice.IntegerValue,
                0d,
                0d,
                trade.InstantBuySellContent.Quantity,
                0),
            TradeType.ManualBuy => new TradeAnalyticsValueBreakdown(
                0d,
                trade.InstantBuySellContent.TotalPrice.IntegerValue,
                0d,
                0,
                trade.InstantBuySellContent.Quantity),
            TradeType.Crafting => new TradeAnalyticsValueBreakdown(
                0d,
                trade.InstantBuySellContent.TotalPrice.IntegerValue,
                0d,
                0,
                trade.InstantBuySellContent.Quantity),
            TradeType.PlayerTradeIncoming => new TradeAnalyticsValueBreakdown(
                trade.PlayerTradeContent.IsSilver ? trade.PlayerTradeContent.Silver.IntegerValue : 0d,
                0d,
                0d,
                trade.PlayerTradeContent.IsSilver ? 0 : trade.PlayerTradeContent.Quantity,
                0),
            TradeType.PlayerTradeOutgoing => new TradeAnalyticsValueBreakdown(
                0d,
                trade.PlayerTradeContent.IsSilver ? trade.PlayerTradeContent.Silver.IntegerValue : 0d,
                0d,
                0,
                trade.PlayerTradeContent.IsSilver ? 0 : trade.PlayerTradeContent.Quantity),
            _ => TradeAnalyticsValueBreakdown.Empty
        };
    }

    private static bool IsRelevantForAnalytics(Trade trade)
    {
        if (trade.Type != TradeType.Mail)
        {
            return true;
        }

        return trade.MailType is not MailType.MarketplaceBuyOrderExpired and not MailType.MarketplaceSellOrderExpired;
    }

    private static int GetMailQuantity(Trade trade)
    {
        if (trade.MailContent.UsedQuantity > 0)
        {
            return trade.MailContent.UsedQuantity;
        }

        return trade.MailContent.Quantity;
    }
}