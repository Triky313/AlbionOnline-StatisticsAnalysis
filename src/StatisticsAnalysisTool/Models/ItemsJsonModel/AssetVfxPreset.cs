using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Models.ItemsJsonModel;

public class AssetVfxPreset
{
    [JsonPropertyName("@name")]
    public string Name { get; set; }
}