using System.Windows;

namespace StatisticsAnalysisTool.Dungeon.Models;

public sealed class DungeonRunMetric
{
    public required string Label { get; init; }
    public required string IconPath { get; init; }
    public double Value { get; init; }
    public double ValuePerHour { get; init; }
    public Visibility IconVisibility => string.IsNullOrWhiteSpace(IconPath) ? Visibility.Collapsed : Visibility.Visible;
}