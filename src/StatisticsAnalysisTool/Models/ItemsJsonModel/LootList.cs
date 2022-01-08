using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Models.ItemWindowModel;

public class LootList
{
    [JsonPropertyName("loot")]
    public object Loot { get; set; }
}