using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Models;
using System;
using System.Collections.Generic;

namespace StatisticsAnalysisTool.DamageMeter;

public sealed class MobDamageMeterFragmentDto
{
    public Guid MobInstanceId { get; set; }
    public long MobObjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string UniqueName { get; set; } = string.Empty;
    public string ClusterName { get; set; } = string.Empty;
    public long Damage { get; set; }
    public double DamagePercentage { get; set; }
    public DateTime FirstAttackTime { get; set; }
    public TimeSpan CombatTime { get; set; }
    public double Dps { get; set; }
    public short MobTier { get; set; }
    public string MobType { get; set; } = string.Empty;
    public string MobRank { get; set; } = string.Empty;
    public DashboardContentType ContentType { get; set; }
    public string MapName { get; set; } = string.Empty;
    public Tier MapTier { get; set; } = Tier.Unknown;
    public List<DamageMeterSnapshotFragmentDto> Players { get; set; } = [];
}