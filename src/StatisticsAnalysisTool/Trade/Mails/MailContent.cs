using StatisticsAnalysisTool.Common;
using System.Text.Json.Serialization;
using System.Windows;

namespace StatisticsAnalysisTool.Trade.Mails;

public class MailContent
{
    public int UsedQuantity { get; init; }
    public int Quantity { get; init; }
    public string UniqueItemName { get; init; }
    public long InternalTotalPriceWithoutTax { get; init; }
    public long InternalUnitPricePaidWithOverpayment { get; init; }
    public long InternalTotalDistanceFee { get; init; }
    public double TaxRate { get; set; }
    public double TaxSetupRate { get; set; }

    [JsonIgnore]
    public bool IsTaxesStated => TaxRate > 0;
    [JsonIgnore]
    public FixPoint TotalDistanceFee => FixPoint.FromInternalValue(InternalTotalDistanceFee);
    [JsonIgnore]
    public FixPoint UnitPriceWithoutTax => FixPoint.FromInternalValue(InternalTotalPriceWithoutTax).DoubleValue <= 0 || UsedQuantity <= 0
        ? FixPoint.FromFloatingPointValue(0)
        : FixPoint.FromFloatingPointValue(FixPoint.FromInternalValue(InternalTotalPriceWithoutTax).DoubleValue / UsedQuantity);
    [JsonIgnore]
    public FixPoint TaxPrice => FixPoint.FromFloatingPointValue(FixPoint.FromInternalValue(InternalTotalPriceWithoutTax).DoubleValue / 100 * TaxRate);
    public FixPoint TaxSetupPrice => FixPoint.FromFloatingPointValue(FixPoint.FromInternalValue(InternalTotalPriceWithoutTax).DoubleValue / 100 * TaxSetupRate);
    [JsonIgnore]
    public FixPoint TotalPrice => FixPoint.FromInternalValue(InternalTotalPriceWithoutTax);
    [JsonIgnore]
    public FixPoint TotalPriceWithAddedTaxes => FixPoint.FromFloatingPointValue(FixPoint.FromInternalValue(InternalTotalPriceWithoutTax).DoubleValue * ((100 + TaxRate + TaxSetupRate) / 100));
    [JsonIgnore]
    public FixPoint TotalPriceWithDeductedTaxes => FixPoint.FromFloatingPointValue(FixPoint.FromInternalValue(InternalTotalPriceWithoutTax).DoubleValue * ((100 - TaxRate - TaxSetupRate) / 100));
    [JsonIgnore]
    public FixPoint Overpaid => FixPoint.FromInternalValue((InternalUnitPricePaidWithOverpayment * UsedQuantity) - InternalTotalPriceWithoutTax);
    [JsonIgnore]
    public bool IsMailWithoutValues => InternalTotalPriceWithoutTax == 0 && UsedQuantity == 0;
    [JsonIgnore]
    public Visibility DistanceFeeAboveZeroVisibility => InternalTotalDistanceFee > 0 ? Visibility.Visible : Visibility.Collapsed;

    public string GetAsCsv()
    {
        return $"{UsedQuantity};{Quantity};{UniqueItemName ?? ""};{TotalPrice};{UnitPriceWithoutTax};{TotalDistanceFee};{TaxRate};{TaxSetupRate}";
    }
}