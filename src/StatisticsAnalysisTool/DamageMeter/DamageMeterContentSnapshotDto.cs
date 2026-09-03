using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.DamageMeter;

public sealed class DamageMeterContentSnapshotDto
{
    public List<DamageMeterSnapshotFragmentDto> DamageMeter { get; set; } = [];
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<MobDamageMeterFragmentDto> MobDamageMeter { get; set; }
    public DamageStatsSnapshot DamageStats { get; set; }
    public DamageMeterYourStatsSnapshot YourStats { get; set; }
}