using System.Collections.Generic;

namespace StatisticsAnalysisTool.Models;

public sealed class DashboardCombatPlayerItem(
    string name,
    IReadOnlyList<Item> equipment,
    double estimatedValue)
{
    public string Name { get; } = name;
    public IReadOnlyList<Item> Equipment { get; } = equipment;
    public double EstimatedValue { get; } = estimatedValue;
}