namespace StatisticsAnalysisTool.Models;

public class TemporaryTreasure
{
    public int ObjectId { get; init; }
    public string UniqueName { get; init; }
    public string UniqueNameWithLocation { get; set; }
    public bool AlreadyScanned { get; set; }
}