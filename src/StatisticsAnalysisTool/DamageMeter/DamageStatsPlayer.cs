using System;
using System.Collections.Generic;

namespace StatisticsAnalysisTool.DamageMeter;

internal sealed class DamageStatsPlayer
{
    public Guid PlayerGuid { get; init; }
    public string PlayerName { get; set; }
    public long BiggestHit { get; set; }
    public long BiggestHeal { get; set; }
    public long Overheal { get; set; }
    public HashSet<long> LastHitTargetObjectIds { get; } = [];
    public HashSet<long> AttackedTargetObjectIds { get; } = [];
}