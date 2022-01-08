using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Models.ItemWindowModel;

public class SocketPreset
{
    [JsonPropertyName("@name")]
    public string Name { get; set; }
}