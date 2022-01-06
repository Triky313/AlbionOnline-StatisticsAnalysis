using Newtonsoft.Json;

namespace StatisticsAnalysisTool.Models.ItemWindowModel;

public class SocketPreset
{
    [JsonProperty("@name")]
    public string Name { get; set; }
}