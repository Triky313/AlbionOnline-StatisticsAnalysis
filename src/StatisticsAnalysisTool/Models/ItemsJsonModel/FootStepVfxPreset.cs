using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Models.ItemWindowModel;

public class FootStepVfxPreset
{
    [JsonPropertyName("@name")]
    public string Name { get; set; }
}