namespace StatisticAnalysisTool.Extractor;

public class ItemContainer : IdContainer
{
    public string LocalizationNameVariable { get; set; }
    public string LocalizationDescriptionVariable { get; set; }
    public Dictionary<string, string> LocalizedNames { get; set; }
    public Dictionary<string, string> LocalizedDescriptions { get; set; }
}