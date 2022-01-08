using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Models.ItemWindowModel;

public class AssetVfxPreset
{
    [JsonPropertyName("@name")]
    public string Name { get; set; }
}