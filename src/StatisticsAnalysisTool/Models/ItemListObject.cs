using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Models;

public class ItemListObject
{
    [JsonPropertyName("LocalizationNameVariable")]
    public string LocalizationNameVariable { get; set; }

    [JsonPropertyName("LocalizationDescriptionVariable")]
    public string LocalizationDescriptionVariable { get; set; }

    [JsonPropertyName("LocalizedNames")] 
    public LocalizedNames LocalizedNames { get; set; }

    [JsonPropertyName("Index")] 
    public int Index { get; set; }

    [JsonPropertyName("UniqueName")] 
    public string UniqueName { get; set; }
}