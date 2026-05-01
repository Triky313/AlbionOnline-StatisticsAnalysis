using StatisticsAnalysisTool.GameFileData.Converter;
using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.GameFileData.Models;

public class LootListReferenceJsonObject
{
    [JsonPropertyName("@name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("@chance")]
    [JsonConverter(typeof(FlexibleDoubleJsonConverter))]
    public double Chance { get; set; }

    [JsonPropertyName("@weight")]
    [JsonConverter(typeof(FlexibleDoubleJsonConverter))]
    public double Weight { get; set; }
}
