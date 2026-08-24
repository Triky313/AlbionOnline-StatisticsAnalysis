namespace StatisticsAnalysisTool.Models;

using StatisticsAnalysisTool.Enumerations;

public class TemporaryTreasure
{
    public int ObjectId { get; init; }
    public string UniqueName { get; init; }
    public string UniqueNameWithLocation { get; set; }
    public TreasureRarity Rarity { get; init; }
    public bool AlreadyScanned { get; set; }
}