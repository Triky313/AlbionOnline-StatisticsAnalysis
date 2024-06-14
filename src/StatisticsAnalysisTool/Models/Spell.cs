using StatisticsAnalysisTool.GameFileData;

namespace StatisticsAnalysisTool.Models;

public class Spell
{
    public Spell(int index)
    {
        Index = index;

        var spellGameFileData = SpellData.GetSpellByIndex(Index);
        UniqueName = spellGameFileData.UniqueName;
        Target = spellGameFileData.Target;
        Category = spellGameFileData.Category;

    }

    public int Index { get; init; }
    public string UniqueName { get; init; }
    public string Target { get; init; }
    public string Category { get; init; }
}