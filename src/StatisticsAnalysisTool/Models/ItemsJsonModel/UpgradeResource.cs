using Newtonsoft.Json;

namespace StatisticsAnalysisTool.Models.ItemWindowModel;

public class UpgradeResource
{
    [JsonProperty("@uniquename")]
    public string UniqueName { get; set; }

    [JsonProperty("@count")]
    public string Count { get; set; }
}