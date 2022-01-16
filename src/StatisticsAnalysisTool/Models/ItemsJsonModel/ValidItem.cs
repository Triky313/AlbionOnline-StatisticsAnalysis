using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Models.ItemsJsonModel;

public class ValidItem
{
    [JsonPropertyName("@id")]
    public string Id { get; set; }
}