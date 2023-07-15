using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.GameData.Models;

public class Marker
{
    [JsonPropertyName("@type")]
    public string Type { get; set; }
}