using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Models.ItemWindowModel;

public class AudioInfo
{
    [JsonPropertyName("@name")]
    public string Name { get; set; }
}