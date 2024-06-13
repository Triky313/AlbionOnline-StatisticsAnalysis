namespace StatisticsAnalysisTool.DamageMeter;

public class SpellFragmentDto
{
    public int Index { get; set; }
    public string UniqueName { get; set; }
    public long DamageHealValue { get; set; }
    public string DamageHealShortString { get; set; }
    public string Target { get; set; }
    public string Category { get; set; }
}