using Newtonsoft.Json;

namespace StatisticsAnalysisTool.Models.ItemWindowModel;

public class AudioInfo
{
    [JsonProperty("@name")]
    public string Name { get; set; }
}