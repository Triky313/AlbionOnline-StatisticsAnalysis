using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.GameFileData.Models;

public class Marker
{
    [JsonPropertyName("@type")]
    public string Type { get; set; }
}