using StatisticsAnalysisTool.Common;

namespace StatisticsAnalysisTool.DamageMeter;

public sealed class DamageMeterYourStatsTopEntry
{
    public int Rank
    {
        get; init;
    }

    public string Name
    {
        get; init;
    } = string.Empty;

    public long Value
    {
        get; init;
    }

    public string ValueString => Value.ToShortNumberString();
}