using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.GameData.Models;

public class WorldJsonRootObject
{
    [JsonPropertyName("world")]
    public WorldJsonClustersObject World { get; set; }
}