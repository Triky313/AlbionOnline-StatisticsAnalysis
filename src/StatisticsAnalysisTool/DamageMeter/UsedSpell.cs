using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.GameFileData;

namespace StatisticsAnalysisTool.DamageMeter;

public class UsedSpell
{
    public UsedSpell(int spellIndex, int itemIndex)
    {
        SpellIndex = spellIndex;
        ItemIndex = itemIndex;

        var spellGameFileData = SpellData.GetSpellByIndex(SpellIndex);
        UniqueName = spellGameFileData.UniqueName;
        Target = spellGameFileData.Target;
        Category = spellGameFileData.Category;
    }

    public int SpellIndex { get; init; }
    public int ItemIndex { get; init; }
    public string UniqueName { get; init; }
    public string Target { get; init; }
    public string Category { get; init; }

    public HealthChangeType HealthChangeType { get; set; }
    public long DamageHealValue { get; set; }
    public int Ticks { get; set; }
}