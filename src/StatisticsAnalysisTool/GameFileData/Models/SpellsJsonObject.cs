using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.GameFileData.Models;

public class SpellsJsonObject
{
    [JsonPropertyName("@uniquename")]
    public string UniqueName { get; set; }
    [JsonPropertyName("@target")]
    public string Target { get; set; }
    [JsonPropertyName("@category")]
    public string Category { get; set; }
}