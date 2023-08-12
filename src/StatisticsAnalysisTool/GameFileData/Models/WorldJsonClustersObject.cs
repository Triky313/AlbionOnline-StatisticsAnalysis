using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.GameFileData.Models;

public class WorldJsonClustersObject
{
    [JsonPropertyName("clusters")]
    public WorldJsonClusterObject Clusters { get; set; }
}