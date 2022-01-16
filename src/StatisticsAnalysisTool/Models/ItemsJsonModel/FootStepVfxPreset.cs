using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Models.ItemsJsonModel;

public class FootStepVfxPreset
{
    [JsonPropertyName("@name")]
    public string Name { get; set; }
}