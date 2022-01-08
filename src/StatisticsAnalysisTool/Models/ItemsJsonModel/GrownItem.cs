using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Models.ItemWindowModel;

public class GrownItem
{
    [JsonPropertyName("@uniquename")]
    public string UniqueName { get; set; }

    [JsonPropertyName("@growtime")]
    public string GrowTime { get; set; }

    [JsonPropertyName("@fame")]
    public string Fame { get; set; }

    [JsonPropertyName("offspring")]
    public Offspring OffSpring { get; set; }
}