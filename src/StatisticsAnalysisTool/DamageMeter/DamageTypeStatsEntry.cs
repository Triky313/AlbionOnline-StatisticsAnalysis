using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Localization;

namespace StatisticsAnalysisTool.DamageMeter;

public sealed class DamageTypeStatsEntry
{
    public int Rank { get; init; }
    public DamageType DamageType { get; init; }
    public long Value { get; init; }
    public string ValueString => Value.ToShortNumberString();
    public double BarPercentage { get; init; }
    public double SharePercentage { get; init; }
    public string SharePercentageString => $"{SharePercentage:N1}%";

    public string DisplayName => DamageType switch
    {
        DamageType.Physical => LocalizationController.Translation("PHYSICAL_DAMAGE"),
        DamageType.Magic => LocalizationController.Translation("MAGIC_DAMAGE"),
        DamageType.True => LocalizationController.Translation("TRUE_DAMAGE"),
        _ => string.Empty
    };
}