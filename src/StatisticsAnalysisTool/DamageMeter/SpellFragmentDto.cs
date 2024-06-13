using StatisticsAnalysisTool.Enumerations;

namespace StatisticsAnalysisTool.DamageMeter;

public class SpellFragmentDto
{
    public int SpellIndex { get; set; }
    public int ItemIndex { get; set; }
    public string UniqueName { get; set; }
    public long DamageHealValue { get; set; }
    public string DamageHealShortString { get; set; }
    public string Target { get; set; }
    public string Category { get; set; }
    public int Ticks { get; set; }
    public HealthChangeType HealthChangeType { get; set; }
}