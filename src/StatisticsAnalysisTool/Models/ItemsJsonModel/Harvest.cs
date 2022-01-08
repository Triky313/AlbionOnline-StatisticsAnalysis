using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Models.ItemWindowModel;

public class Harvest
{
    [JsonPropertyName("@growtime")]
    public string GrowTime { get; set; }

    [JsonPropertyName("@lootlist")]
    public string LootList { get; set; }

    [JsonPropertyName("@lootchance")]
    public string LootChance { get; set; }

    [JsonPropertyName("@fame")]
    public string Fame { get; set; }

    [JsonPropertyName("seed")]
    public Seed Seed { get; set; }
}