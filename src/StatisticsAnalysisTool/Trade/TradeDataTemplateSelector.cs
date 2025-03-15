using StatisticsAnalysisTool.Enumerations;
using System.Windows;
using System.Windows.Controls;

namespace StatisticsAnalysisTool.Trade;

public class TradeDataTemplateSelector : DataTemplateSelector
{
    public DataTemplate MailMarketplaceBuyOrderExpiredTemplate { get; set; }
    public DataTemplate MailMarketplaceBuyOrderFinishedTemplate { get; set; }
    public DataTemplate MailMarketplaceSellOrderExpiredTemplate { get; set; }
    public DataTemplate MailMarketplaceSellOrderFinishedTemplate { get; set; }
    public DataTemplate InstantSellTemplate { get; set; }
    public DataTemplate InstantBuyTemplate { get; set; }
    public DataTemplate ManualSellTemplate { get; set; }
    public DataTemplate ManualBuyTemplate { get; set; }
    public DataTemplate CraftingTemplate { get; set; }

    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
        if (item is not Trade trade)
        {
            return base.SelectTemplate(item, container);
        }

        if (trade.Type == TradeType.Mail)
        {
            return trade.MailType switch
            {
                MailType.MarketplaceBuyOrderExpired => MailMarketplaceBuyOrderExpiredTemplate,
                MailType.MarketplaceBuyOrderFinished => MailMarketplaceBuyOrderFinishedTemplate,
                MailType.MarketplaceSellOrderExpired => MailMarketplaceSellOrderExpiredTemplate,
                MailType.MarketplaceSellOrderFinished => MailMarketplaceSellOrderFinishedTemplate,
                _ => base.SelectTemplate(item, container)
            };
        }

        return trade.Type switch
        {
            TradeType.InstantBuy => InstantBuyTemplate,
            TradeType.InstantSell => InstantSellTemplate,
            TradeType.ManualSell => ManualSellTemplate,
            TradeType.ManualBuy => ManualBuyTemplate,
            TradeType.Crafting => CraftingTemplate,
            _ => base.SelectTemplate(item, container)
        };
    }
}