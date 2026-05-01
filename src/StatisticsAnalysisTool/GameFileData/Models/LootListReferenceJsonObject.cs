using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.GameFileData.Models;

public class LootListReferenceJsonObject
{
    [JsonPropertyName("@name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("@chance")]
    public double Chance { get; set; }
}
