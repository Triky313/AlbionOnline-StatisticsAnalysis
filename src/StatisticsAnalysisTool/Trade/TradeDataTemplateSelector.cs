using StatisticsAnalysisTool.Trade.Mails;
using StatisticsAnalysisTool.Trade.Market;
using System.Windows.Controls;
using System.Windows;

namespace StatisticsAnalysisTool.Trade;

public class TradeDataTemplateSelector : DataTemplateSelector
{
    public DataTemplate MailTemplate { get; set; }
    public DataTemplate InstantSellTemplate { get; set; }
    public DataTemplate InstantBuyTemplate { get; set; }

    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
        return item switch
        {
            Mail => MailTemplate,
            InstantSell => InstantSellTemplate,
            InstantBuy => InstantBuyTemplate,
            _ => base.SelectTemplate(item, container)
        };
    }
}