using Newtonsoft.Json;

namespace StatisticsAnalysisTool.Models.ItemWindowModel;

public class CanHarvest
{
    [JsonProperty("@resourcetype")]
    public string ResourceType { get; set; }
}