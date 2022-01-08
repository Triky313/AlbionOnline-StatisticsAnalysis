using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Models.ItemWindowModel;

public class ValidItem
{
    [JsonPropertyName("@id")]
    public string Id { get; set; }
}