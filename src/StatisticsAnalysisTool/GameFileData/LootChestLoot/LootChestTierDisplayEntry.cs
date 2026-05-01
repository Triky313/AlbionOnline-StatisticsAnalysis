using System.Collections.Generic;

namespace StatisticsAnalysisTool.GameFileData.LootChestLoot;

public class LootChestTierDisplayEntry
{
    public int Tier { get; init; }

    public IReadOnlyList<LootPoolDisplayEntry> Pools { get; init; } = [];

    public string Header => Tier > 0
        ? $"Tier {Tier}"
        : "All Tiers";
}
