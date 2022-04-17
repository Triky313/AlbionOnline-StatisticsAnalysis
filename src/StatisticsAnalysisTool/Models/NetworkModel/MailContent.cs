using System.Text.Json.Serialization;
using StatisticsAnalysisTool.Common;

namespace StatisticsAnalysisTool.Models.NetworkModel;

public class MailContent
{
    public int UsedQuantity { get; set; }
    public int Quantity { get; set; }
    public string UniqueItemName { get; set; }
    public long InternalTotalPrice { get; set; }
    public long InternalUnitPrice { get; set; }
    [JsonIgnore]
    public FixPoint TotalPrice => FixPoint.FromInternalValue(InternalTotalPrice);
    [JsonIgnore]
    public FixPoint UnitPrice => FixPoint.FromInternalValue(InternalUnitPrice);
}