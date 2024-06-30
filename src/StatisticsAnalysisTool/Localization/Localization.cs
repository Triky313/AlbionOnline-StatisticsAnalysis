using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Localization;

internal class Localization
{
    [JsonPropertyName("translations")]
    public List<Translation> Translations { get; set; }
}