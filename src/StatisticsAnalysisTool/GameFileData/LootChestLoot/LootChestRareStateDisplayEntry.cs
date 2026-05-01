using System.Collections.Generic;

namespace StatisticsAnalysisTool.GameFileData.LootChestLoot;

public class LootChestRareStateDisplayEntry
{
    public string State { get; init; } = string.Empty;

    public double Weight { get; init; }

    public double RelativeWeight { get; init; }

    public IReadOnlyList<LootChestTierDisplayEntry> Tiers { get; init; } = [];

    public string Header => $"{State} - Weight {Weight:0.####} - Relative {RelativeWeight:0.##}%";
}
