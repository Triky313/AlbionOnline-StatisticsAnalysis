using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Models.ItemWindowModel;

public class CanHarvest
{
    [JsonPropertyName("@resourcetype")]
    public string ResourceType { get; set; }
}