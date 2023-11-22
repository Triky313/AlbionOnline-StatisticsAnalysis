using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Dungeon.Models;

public class MistsMaps
{
    [JsonPropertyName("mapset")]
    public List<MapSet> MapSet { get; set; }
}