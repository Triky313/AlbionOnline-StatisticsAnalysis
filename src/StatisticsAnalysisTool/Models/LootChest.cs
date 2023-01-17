using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Models;

public class LootChest
{
    [JsonPropertyName("@uniquename")]
    public string UniqueName { get; set; }

    [JsonPropertyName("@faction")]
    public string Faction { get; set; }
}