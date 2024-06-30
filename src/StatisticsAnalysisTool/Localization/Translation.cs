using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Localization;

internal class Translation
{
    [JsonPropertyName("tuid")]
    public string TuId { get; set; }

    [JsonPropertyName("tuv")]
    public List<TranslationValue> Tuv { get; set; }
}