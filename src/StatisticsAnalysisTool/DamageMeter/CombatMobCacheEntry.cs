using StatisticsAnalysisTool.GameFileData.Models;
using System;

namespace StatisticsAnalysisTool.DamageMeter;

public sealed class CombatMobCacheEntry
{
    public Guid MobInstanceId { get; init; } = Guid.NewGuid();
    public string ClusterKey { get; init; }
    public string ClusterName { get; init; }
    public long MobObjectId { get; init; }
    public int MobIndex { get; set; }
    public string UniqueName { get; set; }
    public string TypeId { get; set; }
    public double Health { get; set; }
    public double MaxHealth { get; set; }
    public DateTime FirstSeen { get; init; }
    public DateTime LastUpdated { get; set; }
    public MobJsonObject MobData { get; set; }
    public bool IsProvisional { get; set; }

    internal CombatMobCacheEntry Clone()
    {
        return new CombatMobCacheEntry
        {
            MobInstanceId = MobInstanceId,
            ClusterKey = ClusterKey,
            ClusterName = ClusterName,
            MobObjectId = MobObjectId,
            MobIndex = MobIndex,
            UniqueName = UniqueName,
            TypeId = TypeId,
            Health = Health,
            MaxHealth = MaxHealth,
            FirstSeen = FirstSeen,
            LastUpdated = LastUpdated,
            MobData = MobData,
            IsProvisional = IsProvisional
        };
    }
}