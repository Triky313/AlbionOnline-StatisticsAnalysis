using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Models.ItemsJsonModel;

public class Container
{
    [JsonPropertyName("@capacity")]
    public string Capacity { get; set; }

    [JsonPropertyName("@weightlimit")]
    public string WeightLimit { get; set; }
}