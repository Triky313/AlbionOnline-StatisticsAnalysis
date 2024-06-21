using StatisticsAnalysisTool.Common;
using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Trade.Mails;

public class MailContent
{
    public int UsedQuantity { get; init; }
    public int Quantity { get; init; }
    public string UniqueItemName { get; init; }
    public long InternalTotalPrice { get; init; }
    public long InternalUnitPrice { get; init; }
    public double TaxRate { get; set; }
    public double TaxSetupRate { get; set; }
    [JsonIgnore]
    public bool IsTaxesStated => TaxRate > 0;
    [JsonIgnore]
    public FixPoint TotalPrice => FixPoint.FromInternalValue(InternalTotalPrice);
    [JsonIgnore]
    public FixPoint UnitPrice => FixPoint.FromInternalValue(InternalUnitPrice);
    [JsonIgnore]
    public FixPoint ActualUnitPrice => FixPoint.FromInternalValue(InternalTotalPrice).DoubleValue <= 0 || UsedQuantity <= 0
        ? FixPoint.FromFloatingPointValue(0)
        : FixPoint.FromFloatingPointValue(FixPoint.FromInternalValue(InternalTotalPrice).DoubleValue / UsedQuantity);
    [JsonIgnore]
    public FixPoint TaxPrice => FixPoint.FromFloatingPointValue(FixPoint.FromInternalValue(InternalTotalPrice).DoubleValue / 100 * TaxRate);
    public FixPoint TaxSetupPrice => FixPoint.FromFloatingPointValue(FixPoint.FromInternalValue(InternalTotalPrice).DoubleValue / 100 * TaxSetupRate);
    [JsonIgnore]
    public FixPoint TotalPriceWithDeductedTaxes => FixPoint.FromFloatingPointValue(FixPoint.FromInternalValue(InternalTotalPrice).DoubleValue * ((100 - TaxRate - TaxSetupRate) / 100));
    [JsonIgnore]
    public FixPoint UnitPriceWithDeductedTaxes => FixPoint.FromFloatingPointValue(FixPoint.FromInternalValue(InternalUnitPrice).DoubleValue * ((100 - TaxRate - TaxSetupRate) / 100));
    [JsonIgnore]
    public bool IsMailWithoutValues => InternalTotalPrice == 0 && UsedQuantity == 0;

    public string GetAsCsv()
    {
        return $"{UsedQuantity};{Quantity};{UniqueItemName ?? ""};{TotalPrice};{UnitPrice};{TaxRate};{TaxSetupRate}";
    }
}