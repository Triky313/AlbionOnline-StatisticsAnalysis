using System;

namespace StatisticsAnalysisTool.DamageMeter;

internal sealed class DamageStatsEvent
{
    public DateTime Timestamp { get; init; }
    public long PlayerObjectId { get; init; }
    public long TargetObjectId { get; init; }
    public long Value { get; init; }
}