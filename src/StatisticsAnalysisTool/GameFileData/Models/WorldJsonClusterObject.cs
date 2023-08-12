using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.GameFileData.Models;

public class WorldJsonClusterObject
{
    [JsonPropertyName("cluster")]
    public List<WorldJsonObject> Cluster { get; set; }
}