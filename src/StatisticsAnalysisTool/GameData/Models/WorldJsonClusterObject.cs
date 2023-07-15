using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.GameData.Models;

public class WorldJsonClusterObject
{
    [JsonPropertyName("cluster")]
    public List<WorldJsonObject> Cluster { get; set; }
}