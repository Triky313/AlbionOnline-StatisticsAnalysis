using System.Windows.Media;

namespace StatisticsAnalysisTool.Models;

public sealed class DashboardContentRankingItem(string name, double value, double sharePercentage, double barPercentage, Brush brush)
{
    public string Name { get; } = name;
    public double Value { get; } = value;
    public double SharePercentage { get; } = sharePercentage;
    public double BarPercentage { get; } = barPercentage;
    public Brush Brush { get; } = brush;
}