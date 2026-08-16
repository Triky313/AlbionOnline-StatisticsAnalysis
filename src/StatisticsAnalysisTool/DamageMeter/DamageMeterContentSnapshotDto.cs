using System.Collections.Generic;

namespace StatisticsAnalysisTool.DamageMeter;

public sealed class DamageMeterContentSnapshotDto
{
    public List<DamageMeterSnapshotFragmentDto> DamageMeter { get; set; } = [];
    public DamageStatsSnapshot DamageStats { get; set; }
    public DamageMeterYourStatsSnapshot YourStats { get; set; }
}