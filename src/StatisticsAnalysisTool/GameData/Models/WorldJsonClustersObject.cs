using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.GameData.Models;

public class WorldJsonClustersObject
{
    [JsonPropertyName("clusters")]
    public WorldJsonClusterObject Clusters { get; set; }
}