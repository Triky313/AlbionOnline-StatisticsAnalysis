namespace StatisticsAnalysisTool.Crafting;

public sealed class LossExplorerEventItem
{
    public string ItemUniqueName { get; init; } = string.Empty;
    public int Count { get; init; }
    public int QualityLevel { get; init; }
}