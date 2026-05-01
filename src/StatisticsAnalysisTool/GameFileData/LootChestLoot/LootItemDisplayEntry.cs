using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Models;
using System.Windows.Media.Imaging;

namespace StatisticsAnalysisTool.GameFileData.LootChestLoot;

public class LootItemDisplayEntry
{
    public string UniqueName { get; init; } = string.Empty;

    public string Amount { get; init; } = string.Empty;

    public double Chance { get; init; }

    public double Weight { get; init; }

    public double RelativeWeight { get; init; }

    public string SourcePath { get; init; } = string.Empty;

    public Item Item => ItemController.GetItemByUniqueName(UniqueName);

    public string LocalizedName => Item?.LocalizedName ?? UniqueName;

    public BitmapImage Icon => ImageController.GetItemImage(UniqueName, 52, 52, true);

    public string ToolTip
    {
        get
        {
            var chance = Chance > 0
                ? $"\nChance: {Chance:0.######}"
                : string.Empty;
            var weight = Weight > 0
                ? $"\nWeight: {Weight:0.####}"
                : string.Empty;
            var relative = RelativeWeight > 0
                ? $"\nRelative: {RelativeWeight:0.##}%"
                : string.Empty;
            var amount = string.IsNullOrWhiteSpace(Amount)
                ? string.Empty
                : $"\nAmount: {Amount}";

            return $"{LocalizedName}\n{UniqueName}{amount}{chance}{weight}{relative}\nPath: {SourcePath}";
        }
    }
}
