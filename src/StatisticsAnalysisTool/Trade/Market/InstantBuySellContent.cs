using StatisticsAnalysisTool.Common;
using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Trade.Market;

public class InstantBuySellContent
{
    public long InternalUnitPrice { get; init; }
    public int Quantity { get; init; }
    public double TaxRate { get; init; }
    [JsonIgnore]
    public long InternalTotalPrice => InternalUnitPrice * Quantity;
    [JsonIgnore]
    public bool IsTaxesStated => TaxRate > 0;
    [JsonIgnore]
    public FixPoint TotalPrice => FixPoint.FromInternalValue(InternalTotalPrice);
    [JsonIgnore]
    public FixPoint UnitPrice => FixPoint.FromInternalValue(InternalUnitPrice);
    [JsonIgnore]
    public FixPoint TaxPrice => FixPoint.FromFloatingPointValue(FixPoint.FromInternalValue(InternalTotalPrice).DoubleValue / 100 * TaxRate);
    [JsonIgnore]
    public FixPoint TotalPriceWithDeductedTaxes => FixPoint.FromFloatingPointValue(FixPoint.FromInternalValue(InternalTotalPrice).DoubleValue * ((100 - TaxRate) / 100));
    [JsonIgnore]
    public FixPoint UnitPriceWithDeductedTaxes => FixPoint.FromFloatingPointValue(FixPoint.FromInternalValue(InternalUnitPrice).DoubleValue * ((100 - TaxRate) / 100));

    public string GetAsCsv()
    {
        return $"{UnitPrice};{Quantity};{TaxRate}";
    }
}