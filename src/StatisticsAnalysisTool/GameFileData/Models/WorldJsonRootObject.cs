using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.GameFileData.Models;

public class WorldJsonRootObject
{
    [JsonPropertyName("world")]
    public WorldJsonClustersObject World { get; set; }
}