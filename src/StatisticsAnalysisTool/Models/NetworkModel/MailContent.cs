using StatisticsAnalysisTool.Common;

namespace StatisticsAnalysisTool.Models.NetworkModel;

public class MailContent
{
    public MailContent()
    {
    }

    public MailContent(int quantity, string uniqueItemName, FixPoint totalPrice, FixPoint unitPrice)
    {
        Quantity = quantity;
        UniqueItemName = uniqueItemName;
        TotalPrice = totalPrice;
        UnitPrice = unitPrice;
    }

    public int Quantity { get; set; }
    public string UniqueItemName { get; set; }
    public FixPoint TotalPrice { get; set; }
    public FixPoint UnitPrice { get; set; }

}