using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Localization;

internal class TranslationValue
{
    [JsonPropertyName("lang")]
    public string Lang { get; set; }

    [JsonPropertyName("seg")]
    public string Seg { get; set; }
}