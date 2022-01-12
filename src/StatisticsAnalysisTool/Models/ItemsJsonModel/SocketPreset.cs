using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Models.ItemsJsonModel;

public class SocketPreset
{
    [JsonPropertyName("@name")]
    public string Name { get; set; }
}