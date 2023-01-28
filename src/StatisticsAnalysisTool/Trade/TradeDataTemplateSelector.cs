using System.Diagnostics;
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
        var tradeItem = item as Trade;
        //if (tradeItem?.Type == TradeType.Mail)
        //{
        //    return MailTemplate;
        //}

        //if (tradeItem?.Type == TradeType.InstantBuy)
        //{
        //    return InstantBuyTemplate;
        //}

        //if (tradeItem?.Type == TradeType.InstantSell)
        //{
        //    return InstantSellTemplate;
        //}

        //return base.SelectTemplate(item, container);

        Debug.Print("--------------------------------------------------");
        Debug.Print($"Type: {item?.GetType()} | EnumType: {tradeItem?.Type}");
        switch (item)
        {
            case Mail:
                Debug.Print("Case Mail");

                if (item is Mail mail)
                {
                    Debug.Print($"{mail.MailType}");
                }
                return MailTemplate;
            case InstantSell:
                Debug.Print("Case InstantSell");
                if (item is InstantSell instantSell)
                {
                    Debug.Print($"AuctionEntry: {instantSell.AuctionEntry.AuctionType}");
                }
                return InstantSellTemplate;
            case InstantBuy:
                Debug.Print("Case InstantBuy");
                if (item is InstantBuy instantBuy)
                {
                    Debug.Print($"AuctionEntry: {instantBuy.AuctionEntry.AuctionType}");
                }
                return InstantBuyTemplate;
            default:
                Debug.Print(">>> Default");
                return base.SelectTemplate(item, container);
        }
    }
}