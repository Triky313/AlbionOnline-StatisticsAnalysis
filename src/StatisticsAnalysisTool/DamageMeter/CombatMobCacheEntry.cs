using StatisticsAnalysisTool.GameFileData.Models;
using System;

namespace StatisticsAnalysisTool.DamageMeter;

public sealed class CombatMobCacheEntry
{
    public string ClusterKey { get; init; }
    public string ClusterName { get; init; }
    public long MobObjectId { get; init; }
    public int MobIndex { get; set; }
    public string MobName { get; set; }
    public string UniqueName { get; set; }
    public string TypeId { get; set; }
    public string Identifier { get; set; }
    public double Health { get; set; }
    public double MaxHealth { get; set; }
    public DateTime FirstSeen { get; init; }
    public DateTime LastUpdated { get; set; }
    public MobJsonObject MobData { get; set; }
}