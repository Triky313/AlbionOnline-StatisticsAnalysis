using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Dungeon.Models;

public class MistsJsonRootObject
{
    [JsonPropertyName("mists")]
    public Mists Mists { get; set; }
}