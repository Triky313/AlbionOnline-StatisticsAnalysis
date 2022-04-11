using System.Text.Json.Serialization;
using StatisticsAnalysisTool.Common;

namespace StatisticsAnalysisTool.Models.NetworkModel;

public class MailContent
{
    public MailContent()
    {
    }

    public MailContent(int quantity, string uniqueItemName, long internalTotalPrice, long internalUnitPrice)
    {
        Quantity = quantity;
        UniqueItemName = uniqueItemName;
        InternalTotalPrice = internalTotalPrice;
        InternalUnitPrice = internalUnitPrice;
    }

    public int Quantity { get; set; }
    public string UniqueItemName { get; set; }
    public long InternalTotalPrice { get; set; }
    public long InternalUnitPrice { get; set; }
    [JsonIgnore]
    public FixPoint TotalPrice => FixPoint.FromInternalValue(InternalTotalPrice);
    [JsonIgnore]
    public FixPoint UnitPrice => FixPoint.FromInternalValue(InternalUnitPrice);
}