using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Models;

public class WorldJsonObject
{
    [JsonPropertyName("Index")] 
    public string Index { get; set; }

    [JsonPropertyName("UniqueName")] 
    public string UniqueName { get; set; }

    [JsonPropertyName("Type")] 
    public string Type { get; set; }

    [JsonPropertyName("File")] 
    public string File { get; set; }

    [JsonPropertyName("MiniMapMarkers")]
    public MiniMapMarkers MiniMapMarkers { get; set; }
}