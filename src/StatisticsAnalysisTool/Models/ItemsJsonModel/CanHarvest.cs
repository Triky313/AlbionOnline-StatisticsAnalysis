using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Models.ItemsJsonModel;

public class CanHarvest
{
    [JsonPropertyName("@resourcetype")]
    public string ResourceType { get; set; }
}