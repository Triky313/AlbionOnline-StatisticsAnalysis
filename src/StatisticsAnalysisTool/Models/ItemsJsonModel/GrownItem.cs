using Newtonsoft.Json;

namespace StatisticsAnalysisTool.Models.ItemWindowModel;

public class GrownItem
{
    [JsonProperty("@uniquename")]
    public string UniqueName { get; set; }

    [JsonProperty("@growtime")]
    public string GrowTime { get; set; }

    [JsonProperty("@fame")]
    public string Fame { get; set; }

    [JsonProperty("offspring")]
    public Offspring OffSpring { get; set; }
}