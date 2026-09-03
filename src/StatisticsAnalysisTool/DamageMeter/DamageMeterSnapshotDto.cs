using StatisticsAnalysisTool.Models;
using System;
using System.Collections.Generic;

namespace StatisticsAnalysisTool.DamageMeter;

public sealed class DamageMeterSnapshotDto
{
    public DateTime Timestamp { get; set; }
    public string Location { get; set; }
    public bool IsAutoSave { get; set; }
    public List<DamageMeterSnapshotFragmentDto> DamageMeter { get; set; }
    public List<MobDamageMeterFragmentDto> MobDamageMeter { get; set; } = [];
    public List<MobDamageMeterPlayerIdentityDto> MobDamageMeterPlayers { get; set; } = [];
    public DamageStatsSnapshot DamageStats { get; set; }
    public DamageMeterYourStatsSnapshot YourStats { get; set; }
    public Dictionary<DashboardContentType, DamageMeterContentSnapshotDto> ContentSnapshots { get; set; } = [];
}