using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Models.ItemsJsonModel;

public class AudioInfo
{
    [JsonPropertyName("@name")]
    public string Name { get; set; }
}