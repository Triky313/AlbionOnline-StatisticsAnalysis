using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Models.ItemWindowModel;

public class Seed
{
    [JsonPropertyName("@chance")]
    public string Chance { get; set; }

    [JsonPropertyName("@amount")]
    public string Amount { get; set; }
}