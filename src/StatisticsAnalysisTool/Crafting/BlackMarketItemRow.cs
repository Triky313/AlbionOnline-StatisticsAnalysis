using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Globalization;
using System.Windows.Media.Imaging;

namespace StatisticsAnalysisTool.Crafting;

public sealed class BlackMarketItemRow : BaseViewModel
{
    private BlackMarketHistoryEntry _history;

    public BlackMarketItemRow(Item item, int qualityLevel, BlackMarketHistoryEntry history)
    {
        Item = item;
        QualityLevel = qualityLevel;
        Refresh(history);
    }

    public Item Item { get; }

    public string ItemUniqueName => Item?.UniqueName;

    public string ItemName => Item?.LocalizedNameAndEnglish ?? ItemUniqueName ?? string.Empty;

    public int Tier => Item?.Tier ?? -1;

    public int EnchantmentLevel => Item?.Level ?? 0;

    public string TierLevelString => Item?.TierLevelString ?? string.Empty;

    public int QualityLevel { get; }

    public string QualityName => ItemController.GetQuality(QualityLevel) switch
    {
        ItemQuality.Normal => LocalizationController.Translation("NORMAL"),
        ItemQuality.Good => LocalizationController.Translation("GOOD"),
        ItemQuality.Outstanding => LocalizationController.Translation("OUTSTANDING"),
        ItemQuality.Excellent => LocalizationController.Translation("EXCELLENT"),
        ItemQuality.Masterpiece => LocalizationController.Translation("MASTERPIECE"),
        _ => string.Empty
    };

    public BitmapImage Icon => Item?.Icon;

    public ulong CurrentBuyPrice => _history?.CurrentBuyPrice ?? 0;

    public long LatestAveragePrice => _history?.LatestAveragePrice ?? 0;

    public int SoldLast30Days => _history?.SoldLast30Days ?? 0;

    public int SoldLast365Days => _history?.SoldLast365Days ?? 0;

    public string LastUpdatedText => _history?.LastUpdatedUtc > DateTime.MinValue
        ? _history.LastUpdatedUtc.ToLocalTime().ToString("g", CultureInfo.CurrentCulture)
        : string.Empty;

    public void Refresh(BlackMarketHistoryEntry history)
    {
        _history = history;
        OnPropertyChanged(nameof(CurrentBuyPrice));
        OnPropertyChanged(nameof(LatestAveragePrice));
        OnPropertyChanged(nameof(SoldLast30Days));
        OnPropertyChanged(nameof(SoldLast365Days));
        OnPropertyChanged(nameof(LastUpdatedText));
    }
}
