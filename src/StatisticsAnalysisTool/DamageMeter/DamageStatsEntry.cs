using StatisticsAnalysisTool.Common;

namespace StatisticsAnalysisTool.DamageMeter;

public sealed class DamageStatsEntry
{
    public int Rank { get; init; }
    public string PlayerName { get; init; }
    public long Value { get; init; }
    public string ValueString => Value.ToShortNumberString();
    public string Detail { get; init; }
}