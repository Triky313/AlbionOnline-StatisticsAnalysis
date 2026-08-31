using StatisticsAnalysisTool.Enumerations;
using System;

namespace StatisticsAnalysisTool.Models;

public sealed class DashboardFactionOption(CityFaction faction, string name, string resourceName)
{
    public CityFaction Faction { get; } = faction;
    public string Name { get; } = name;
    public Uri SealIconSource { get; } = new($"pack://application:,,,/Assets/seal_{resourceName}.png");
    public Uri StandingIconSource { get; } = new($"pack://application:,,,/Resources/factionflag_{resourceName}.png");
    public Uri CoinIconSource { get; } = new($"pack://application:,,,/Resources/factioncoin_{resourceName}.png");
}