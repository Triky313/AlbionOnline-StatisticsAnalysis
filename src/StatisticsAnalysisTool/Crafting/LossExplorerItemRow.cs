using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.Models;
using System;
using System.Globalization;
using System.Windows.Media.Imaging;

namespace StatisticsAnalysisTool.Crafting;

public sealed class LossExplorerItemRow
{
    private readonly Item _item;

    public LossExplorerItemRow(LossExplorerCachedItem cachedItem, decimal quantity)
    {
        ItemUniqueName = cachedItem.ItemUniqueName;
        QualityLevel = cachedItem.QualityLevel;
        Quantity = quantity;
        UnitValue = cachedItem.UnitValue;
        _item = ItemController.GetItemByUniqueName(ItemUniqueName) ?? ItemController.GetItemByUniqueName(ItemController.GetCleanUniqueName(ItemUniqueName));
    }

    public string ItemUniqueName { get; }

    public string ItemName => _item?.LocalizedName ?? ItemUniqueName;

    public string ShopCategory => _item?.FullItemInformation?.ShopCategory ?? string.Empty;

    public string ShopSubCategory1 => _item?.FullItemInformation?.ShopSubCategory1 ?? string.Empty;

    public int Tier => ItemController.GetItemTier(ItemUniqueName);

    public int EnchantmentLevel => ItemController.GetItemLevel(ItemUniqueName);

    public int QualityLevel { get; }

    public decimal Quantity { get; }

    public ulong UnitValue { get; }

    public decimal TotalValue => Quantity * UnitValue;

    public string TierText => Tier is >= 1 and <= 8 ? $"T{Tier}" : "T?";

    public string EnchantmentText => $".{EnchantmentLevel}";

    public string QualityName => QualityLevel switch
    {
        1 => LocalizationController.Translation("NORMAL"),
        2 => LocalizationController.Translation("GOOD"),
        3 => LocalizationController.Translation("OUTSTANDING"),
        4 => LocalizationController.Translation("EXCELLENT"),
        5 => LocalizationController.Translation("MASTERPIECE"),
        _ => LocalizationController.Translation("UNKNOWN")
    };

    public string TotalValueText => string.Format(
        CultureInfo.CurrentCulture,
        LocalizationController.Translation("LOSS_EXPLORER_VALUE_PER_DAY"),
        TotalValue);

    public string UnitValueText => string.Format(
        CultureInfo.CurrentCulture,
        LocalizationController.Translation("LOSS_EXPLORER_UNIT_VALUE"),
        UnitValue);

    public string QuantityText => string.Format(
        CultureInfo.CurrentCulture,
        LocalizationController.Translation("LOSS_EXPLORER_AMOUNT_PER_DAY"),
        Quantity);

    public BitmapImage Icon => _item?.Icon;
}