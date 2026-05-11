using StatisticsAnalysisTool.ViewModels;
using System;
using System.Text.Json.Serialization;
using System.Windows.Media.Imaging;

namespace StatisticsAnalysisTool.Crafting;

public class CraftingResourceEntry : BaseViewModel
{
    [JsonIgnore]
    public Action ValuesChanged { get; set; }
    public string UniqueName { get; set; }
    public string DisplayName { get; set; }
    public decimal QuantityPerRun { get; set; }
    public double UnitWeight { get; set; }
    public bool IsReturnable { get; set; }
    public decimal? MaxReturnQuantityPerRun { get; set; }
    public CraftingResourceKind ResourceKind { get; set; }
    public string ResourceKindText => ResourceKind.ToString();

    [JsonIgnore]
    public BitmapImage Icon { get; set; }

    public decimal UnitPrice
    {
        get;
        set
        {
            field = value;
            ValuesChanged?.Invoke();
            OnPropertyChanged();
        }
    }

    public decimal GrossQuantity
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public decimal ExpectedReturnQuantity
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public decimal NetQuantity
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public decimal GrossCost
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public decimal NetCost
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }
}