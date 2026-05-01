using System.Collections.Generic;

namespace StatisticsAnalysisTool.GameFileData.LootChestLoot;

public class LootPoolDisplayEntry
{
    public string Name { get; init; } = string.Empty;

    public double Chance { get; init; }

    public double Weight { get; init; }

    public double RelativeWeight { get; init; }

    public IReadOnlyList<LootItemDisplayEntry> Items { get; init; } = [];

    public string Header
    {
        get
        {
            var values = new List<string>();

            if (Chance > 0)
            {
                values.Add($"Chance {Chance:0.######}");
            }

            if (Weight > 0)
            {
                values.Add($"Weight {Weight:0.####}");
            }

            if (RelativeWeight > 0)
            {
                values.Add($"Relative {RelativeWeight:0.##}%");
            }

            values.Add($"Items {Items.Count}");

            return values.Count > 0
                ? $"{Name} - {string.Join(" - ", values)}"
                : Name;
        }
    }
}
