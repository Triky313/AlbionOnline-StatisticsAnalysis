using System;

namespace StatisticsAnalysisTool.DamageMeter;

public sealed class CombatEventContribution
{
    public Guid CombatEventId { get; init; }
    public DateTime Timestamp { get; init; }
    public CombatEventValueType ValueType { get; init; }
    public long SourceObjectId { get; init; }
    public long TargetObjectId { get; init; }
    public long Value { get; init; }
    public int CausingSpellIndex { get; init; }
}