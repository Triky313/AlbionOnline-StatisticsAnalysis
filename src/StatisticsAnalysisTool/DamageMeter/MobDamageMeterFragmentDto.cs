using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Models;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.DamageMeter;

public sealed class MobDamageMeterFragmentDto
{
    public Guid MobInstanceId { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public long MobObjectId { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Name { get; set; }
    public string UniqueName { get; set; } = string.Empty;
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string ClusterName { get; set; }
    public long Damage { get; set; }
    public DateTime FirstAttackTime { get; set; }
    public TimeSpan CombatTime { get; set; }
    public DashboardContentType ContentType { get; set; }
    public string MapName { get; set; } = string.Empty;
    public Tier MapTier { get; set; } = Tier.Unknown;
    public List<MobDamageMeterPlayerDto> Players { get; set; } = [];
}