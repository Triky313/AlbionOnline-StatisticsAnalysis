using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Text.Json.Serialization;
using System.Windows.Media.Imaging;

namespace StatisticsAnalysisTool.Crafting;

public class CraftingResourceEntry : BaseViewModel
{
    [JsonIgnore]
    public Action ValuesChanged
    {
        get;
        set;
    }

    public string UniqueName
    {
        get;
        set;
    }

    public string DisplayName
    {
        get;
        set;
    }

    public decimal QuantityPerRun
    {
        get;
        set;
    }

    public double UnitWeight
    {
        get;
        set;
    }

    public bool IsReturnable
    {
        get;
        set;
    }

    public decimal? MaxReturnQuantityPerRun
    {
        get;
        set;
    }

    public CraftingResourceKind ResourceKind
    {
        get;
        set;
    }
    public string ResourceKindText
    {
        get
        {
            switch (ResourceKind)
            {
                case CraftingResourceKind.Standard:
                    return LocalizationController.Translation("CRAFTING_RESOURCE_KIND_STANDARD");
                case CraftingResourceKind.Artefact:
                    return LocalizationController.Translation("CRAFTING_RESOURCE_KIND_ARTEFACT");
                case CraftingResourceKind.Favor:
                    return LocalizationController.Translation("FAVOR");
                case CraftingResourceKind.Alchemy:
                    return LocalizationController.Translation("CRAFTING_RESOURCE_KIND_ALCHEMY");
                case CraftingResourceKind.AvalonianEnergy:
                    return LocalizationController.Translation("CRAFTING_RESOURCE_KIND_AVALONIAN_ENERGY");
                case CraftingResourceKind.TomeOfInsight:
                    return LocalizationController.Translation("CRAFTING_RESOURCE_KIND_TOME_OF_INSIGHT");
                case CraftingResourceKind.Essence:
                    return LocalizationController.Translation("CRAFTING_RESOURCE_KIND_ESSENCE");
                case CraftingResourceKind.Special:
                    return LocalizationController.Translation("CRAFTING_RESOURCE_KIND_SPECIAL");
                default:
                    return ResourceKind.ToString();
            }
        }
    }

    [JsonIgnore]
    public BitmapImage Icon
    {
        get;
        set;
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