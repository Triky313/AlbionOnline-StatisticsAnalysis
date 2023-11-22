using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.GameFileData.Models;

public class WorldJsonObject
{
    [JsonPropertyName("@id")]
    public string Index { get; set; }

    [JsonPropertyName("@displayname")]
    public string UniqueName { get; set; }

    [JsonPropertyName("@type")]
    public string Type { get; set; }

    [JsonPropertyName("@file")]
    public string File { get; set; }

    [JsonPropertyName("minimapmarkers")]
    public MiniMapMarkers MiniMapMarkers { get; set; }
}