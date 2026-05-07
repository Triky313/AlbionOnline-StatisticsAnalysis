using System;
using System.Collections.Generic;

namespace StatisticsAnalysisTool.DamageMeter;

public sealed class DamageMeterSnapshotDto
{
    public DateTime Timestamp { get; set; }
    public string Location { get; set; }
    public bool IsAutoSave { get; set; }
    public List<DamageMeterSnapshotFragmentDto> DamageMeter { get; set; }
    public DamageStatsSnapshot DamageStats { get; set; }
    public DamageMeterYourStatsSnapshot YourStats { get; set; }
}