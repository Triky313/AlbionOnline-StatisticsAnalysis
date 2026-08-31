using System.Collections.Generic;

namespace StatisticsAnalysisTool.DamageMeter;

public sealed class DamageMeterContentSnapshot
{
    public List<DamageMeterSnapshotFragment> DamageMeter { get; set; } = [];
    public List<MobDamageMeterFragment> MobDamageMeter { get; set; } = [];
    public DamageStatsSnapshot DamageStats { get; set; } = DamageStatsSnapshot.Empty;
    public DamageMeterYourStatsSnapshot YourStats { get; set; } = DamageMeterYourStatsSnapshot.Empty;

    public bool HasData => DamageMeter.Count > 0 || MobDamageMeter.Count > 0;
}