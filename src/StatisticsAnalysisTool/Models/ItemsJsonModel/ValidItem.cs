using Newtonsoft.Json;

namespace StatisticsAnalysisTool.Models.ItemWindowModel;

public class ValidItem
{
    [JsonProperty("@id")]
    public string Id { get; set; }
}