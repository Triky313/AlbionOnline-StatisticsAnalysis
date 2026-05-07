using System.Collections.Generic;

namespace StatisticsAnalysisTool.DamageMeter;

internal sealed class DamageStatsPlayer
{
    public long PlayerObjectId { get; init; }
    public string PlayerName { get; set; }
    public long BiggestHit { get; set; }
    public long BiggestHeal { get; set; }
    public long Overheal { get; set; }
    public HashSet<long> LastHitTargetObjectIds { get; } = [];
    public HashSet<long> AttackedTargetObjectIds { get; } = [];
}