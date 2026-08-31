using System.Collections.Generic;

namespace StatisticsAnalysisTool.Models.ItemDetailsModel;

public sealed class ItemSpellCollection
{
    public ItemSpellCollection(IReadOnlyList<ItemSpellInformation> activeSpells, IReadOnlyList<ItemSpellInformation> passiveSpells)
    {
        ActiveSpells = activeSpells;
        PassiveSpells = passiveSpells;
    }

    public IReadOnlyList<ItemSpellInformation> ActiveSpells { get; }
    public IReadOnlyList<ItemSpellInformation> PassiveSpells { get; }
    public bool HasSpells => ActiveSpells.Count > 0 || PassiveSpells.Count > 0;
}