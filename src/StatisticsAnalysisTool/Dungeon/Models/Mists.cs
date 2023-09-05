using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Dungeon.Models;

public class Mists
{
    [JsonPropertyName("mistsmaps")]
    public MistsMaps MistsMaps { get; set; }
}