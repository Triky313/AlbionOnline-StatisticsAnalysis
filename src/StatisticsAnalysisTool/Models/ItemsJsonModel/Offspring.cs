using Newtonsoft.Json;

namespace StatisticsAnalysisTool.Models.ItemWindowModel;

public class Offspring
{
    [JsonProperty("@chance")]
    public string Chance { get; set; }

    [JsonProperty("@amount")]
    public string Amount { get; set; }
}