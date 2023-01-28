using System.Windows;
using System.Windows.Controls;

namespace StatisticsAnalysisTool.Trade;

public class TradeDataTemplateSelector : DataTemplateSelector
{
    public DataTemplate MailTemplate { get; set; }
    public DataTemplate InstantSellTemplate { get; set; }
    public DataTemplate InstantBuyTemplate { get; set; }

    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
        if (item is not Trade trade)
        {
            return base.SelectTemplate(item, container);
        }

        return trade.Type switch
        {
            TradeType.Mail => MailTemplate,
            TradeType.InstantBuy => InstantBuyTemplate,
            TradeType.InstantSell => InstantSellTemplate,
            _ => base.SelectTemplate(item, container)
        };
    }
}