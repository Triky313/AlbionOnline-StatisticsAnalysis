using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Cluster;
using StatisticsAnalysisTool.Dungeon;
using StatisticsAnalysisTool.Enumerations;
using System;
using ValueType = StatisticsAnalysisTool.Enumerations.ValueType;

namespace StatisticsAnalysisTool.Models;

public sealed class StatisticEntry
{
    public Guid SessionId { get; set; }
    public DateTime OccurredAtUtc { get; set; }
    public ValueType ValueType { get; set; }
    public double Value { get; set; }
    public MapType MapType { get; set; }
    public DungeonMode DungeonMode { get; set; }
    public ClusterMode ClusterMode { get; set; }
    public CityFaction CityFaction { get; set; }
    public int ItemIndex { get; set; }
    public int ItemQuantity { get; set; }
    public string LootAreaIndex { get; set; } = string.Empty;
    public ClusterType LootAreaClusterType { get; set; }
    public DateTime LootAreaEnteredAtUtc { get; set; }
    public TreasureRarity TreasureRarity { get; set; }
    public ItemQuality ItemQuality { get; set; } = ItemQuality.Unknown;
    public string CombatAreaIndex { get; set; } = string.Empty;
    public ClusterType CombatAreaClusterType { get; set; }
    public string CombatOpponentName { get; set; } = string.Empty;
    public double CombatLootValue { get; set; }
    public CombatPlayerSnapshot CombatKiller { get; set; }
    public CombatPlayerSnapshot CombatVictim { get; set; }
    public string MobUniqueName { get; set; } = string.Empty;
}