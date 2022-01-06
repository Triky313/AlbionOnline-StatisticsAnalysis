using Newtonsoft.Json;

namespace StatisticsAnalysisTool.Models.ItemWindowModel;

public class Container
{
    [JsonProperty("@capacity")]
    public string Capacity { get; set; }

    [JsonProperty("@weightlimit")]
    public string WeightLimit { get; set; }
}