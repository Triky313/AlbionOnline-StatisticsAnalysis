using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Models.ItemWindowModel;

public class ItemsJson
{
    [JsonPropertyName("items")]
    public Items Items { get; set; }
}