using StatisticsAnalysisTool.GameFileData.Models;
using System.Collections.Generic;

namespace StatisticsAnalysisTool.GameFileData.LootChestLoot;

public class LootChestDisplayEntry
{
    private readonly LootChestJsonObject _lootChest;
    private readonly LootChestLootResolver _resolver;
    private IReadOnlyList<LootChestRareStateDisplayEntry> _rareStates;

    public LootChestDisplayEntry(LootChestJsonObject lootChest, LootChestLootResolver resolver)
    {
        _lootChest = lootChest;
        _resolver = resolver;
    }

    public string UniqueName => _lootChest.UniqueName;

    public string ChestType => _lootChest.ChestType;

    public string Prefab => _lootChest.Prefab;

    public string Header => string.IsNullOrWhiteSpace(ChestType)
        ? UniqueName
        : $"{UniqueName} ({ChestType})";

    public IReadOnlyList<LootChestRareStateDisplayEntry> RareStates => _rareStates ??= _resolver.ResolveRareStates(_lootChest);
}
