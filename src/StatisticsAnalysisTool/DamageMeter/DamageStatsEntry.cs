using StatisticsAnalysisTool.Common;

namespace StatisticsAnalysisTool.DamageMeter;

public sealed class DamageStatsEntry
{
    public static DamageStatsEntry Empty { get; } = new();

    public int Rank { get; init; }
    public string PlayerName { get; init; } = string.Empty;
    public long Value { get; init; }
    public string ValueString => Value.ToShortNumberString();
    public double BarPercentage { get; init; }
    public double SharePercentage { get; init; }
    public string SharePercentageString => $"{SharePercentage:N1}%";
    public string Detail { get; init; } = string.Empty;
}