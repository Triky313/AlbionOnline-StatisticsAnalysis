using Newtonsoft.Json;

namespace StatisticsAnalysisTool.Models.ItemWindowModel;

public class Harvest
{
    [JsonProperty("@growtime")]
    public string GrowTime { get; set; }

    [JsonProperty("@lootlist")]
    public string LootList { get; set; }

    [JsonProperty("@lootchance")]
    public string LootChance { get; set; }

    [JsonProperty("@fame")]
    public string Fame { get; set; }

    [JsonProperty("seed")]
    public Seed Seed { get; set; }
}