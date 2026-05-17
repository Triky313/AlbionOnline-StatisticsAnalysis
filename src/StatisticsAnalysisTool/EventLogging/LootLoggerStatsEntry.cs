using StatisticsAnalysisTool.Common;
using System.Windows;
using System.Windows.Media.Imaging;

namespace StatisticsAnalysisTool.EventLogging;

public sealed class LootLoggerStatsEntry
{
    public int Rank { get; init; }

    public string Name { get; init; } = string.Empty;

    public double Value { get; init; }

    public string ValueString => Value.ToShortNumberString();

    public string Detail { get; init; } = string.Empty;

    public Visibility DetailVisibility => string.IsNullOrWhiteSpace(Detail) ? Visibility.Collapsed : Visibility.Visible;

    public BitmapImage Icon { get; init; }

    public Visibility IconVisibility => Icon == null ? Visibility.Collapsed : Visibility.Visible;
}