using StatisticsAnalysisTool.Common;
using System.Text.Json.Serialization;
using System.Windows;

namespace StatisticsAnalysisTool.Trade.Market;

public class InstantBuySellContent
{
    public long InternalUnitPrice { get; init; }
    public long InternalDistanceFee { get; init; }
    public int Quantity { get; init; }
    public double TaxRate { get; init; }
    [JsonIgnore]
    public long InternalTotalPrice => (InternalUnitPrice + InternalDistanceFee) * Quantity;
    [JsonIgnore]
    public bool IsTaxesStated => TaxRate > 0;
    [JsonIgnore]
    public FixPoint TotalPrice => FixPoint.FromInternalValue(InternalTotalPrice);
    [JsonIgnore]
    public FixPoint UnitPrice => FixPoint.FromInternalValue(InternalUnitPrice);
    [JsonIgnore]
    public FixPoint DistanceFee => FixPoint.FromInternalValue(InternalDistanceFee);
    [JsonIgnore]
    public FixPoint TotalDistanceFee => FixPoint.FromInternalValue(InternalDistanceFee * Quantity);
    [JsonIgnore]
    public FixPoint TaxPrice => FixPoint.FromFloatingPointValue(FixPoint.FromInternalValue(InternalTotalPrice).DoubleValue / 100 * TaxRate);
    [JsonIgnore]
    public FixPoint TotalPriceWithDeductedTaxes => FixPoint.FromFloatingPointValue(FixPoint.FromInternalValue(InternalTotalPrice).DoubleValue * ((100 - TaxRate) / 100));
    [JsonIgnore]
    public FixPoint UnitPriceWithDeductedTaxes => FixPoint.FromFloatingPointValue(FixPoint.FromInternalValue(InternalUnitPrice).DoubleValue * ((100 - TaxRate) / 100));
    [JsonIgnore]
    public Visibility DistanceFeeAboveZeroVisibility => InternalDistanceFee > 0 ? Visibility.Visible : Visibility.Collapsed;

    public string GetAsCsv()
    {
        return $"{UnitPrice};{Quantity};{DistanceFee};{TaxRate};";
    }
}