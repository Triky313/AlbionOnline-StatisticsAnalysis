using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using System.Windows.Media.Imaging;

namespace StatisticsAnalysisTool.Crafting;

public class CraftingResourceEntry : BaseViewModel
{
    [JsonIgnore]
    public Action ValuesChanged { get; set; }
    public string UniqueName { get; set; }

    [JsonIgnore]
    public string DisplayName => ItemController.GetItemByUniqueName(UniqueName)?.LocalizedName ?? UniqueName ?? string.Empty;

    public decimal QuantityPerRun { get; set; }
    public double UnitWeight { get; set; }
    public bool IsReturnable { get; set; }
    public decimal? MaxReturnQuantityPerRun { get; set; }
    public CraftingResourceKind ResourceKind { get; set; }
    public string ResourceKindText
    {
        get
        {
            return ResourceKind switch
            {
                CraftingResourceKind.Standard => LocalizationController.Translation("CRAFTING_RESOURCE_KIND_STANDARD"),
                CraftingResourceKind.Artefact => LocalizationController.Translation("CRAFTING_RESOURCE_KIND_ARTEFACT"),
                CraftingResourceKind.Favor => LocalizationController.Translation("FAVOR"),
                CraftingResourceKind.Alchemy => LocalizationController.Translation("CRAFTING_RESOURCE_KIND_ALCHEMY"),
                CraftingResourceKind.AvalonianEnergy => LocalizationController.Translation("CRAFTING_RESOURCE_KIND_AVALONIAN_ENERGY"),
                CraftingResourceKind.TomeOfInsight => LocalizationController.Translation("CRAFTING_RESOURCE_KIND_TOME_OF_INSIGHT"),
                CraftingResourceKind.Essence => LocalizationController.Translation("CRAFTING_RESOURCE_KIND_ESSENCE"),
                CraftingResourceKind.Special => LocalizationController.Translation("CRAFTING_RESOURCE_KIND_SPECIAL"),
                _ => ResourceKind.ToString()
            };
        }
    }

    [JsonIgnore]
    public BitmapImage Icon { get; set; }

    [JsonIgnore]
    public ObservableCollection<CraftingSellPriceOption> PriceOptions
    {
        get;
    }
    = [];

    [JsonIgnore]
    public bool IsPricePopupOpen
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

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
