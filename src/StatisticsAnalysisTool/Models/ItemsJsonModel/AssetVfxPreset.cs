using Newtonsoft.Json;

namespace StatisticsAnalysisTool.Models.ItemWindowModel;

public class AssetVfxPreset
{
    [JsonProperty("@name")]
    public string Name { get; set; }
}