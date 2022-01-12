using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Models.ItemsJsonModel;

public class LootList
{
    [JsonPropertyName("loot")]
    public object Loot { get; set; }
}