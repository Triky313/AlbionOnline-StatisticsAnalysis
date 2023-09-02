using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Dungeon.Models;

public class MapSet
{
    [JsonPropertyName("@name")]
    public string Name { get; set; }
    [JsonPropertyName("map")]
    public List<Map> Map { get; set; }
}