using StatisticsAnalysisTool.Common;
using System.Globalization;
using System.Windows;
using System.Windows.Media.Imaging;

namespace StatisticsAnalysisTool.Trade;

public sealed class TradeItemRankingEntry
{
    public int Rank { get; init; }
    public string ItemUniqueName { get; init; } = string.Empty;
    public string ItemName { get; init; } = string.Empty;
    public string TierLevelDisplay { get; init; } = string.Empty;
    public double NetProfit { get; init; }
    public double Roi { get; init; }
    public double InvestedCapital { get; init; }
    public long SoldQuantity { get; init; }
    public long BoughtQuantity { get; init; }
    public int TradeCount { get; init; }
    public double HighlightValue { get; init; }
    public string RankDisplay => $"{Rank}.";
    public BitmapImage ItemIcon => Application.Current.Dispatcher.Invoke(() => ImageController.GetItemImage(ItemUniqueName, 48, 48));
    public string NetProfitDisplay => NetProfit.ToChartTooltipNumberString();
    public string RoiDisplay => $"{Roi.ToString("N2", CultureInfo.CurrentCulture)} %";
    public string SoldQuantityDisplay => SoldQuantity.ToString("N0", CultureInfo.CurrentCulture);
    public string BoughtQuantityDisplay => BoughtQuantity.ToString("N0", CultureInfo.CurrentCulture);
    public string HighlightValueDisplay { get; init; } = string.Empty;
    public bool IsHighlightValueNegative => HighlightValue < 0d;
}
