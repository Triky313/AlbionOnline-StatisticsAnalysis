using System.Collections.Concurrent;

namespace StatisticAnalysisTool.Extractor;

public class ItemContainer : IdContainer
{
    public string LocalizationNameVariable { get; set; }
    public string LocalizationDescriptionVariable { get; set; }
    public ConcurrentDictionary<string, string> LocalizedNames { get; set; }
    public ConcurrentDictionary<string, string> LocalizedDescriptions { get; set; }
}