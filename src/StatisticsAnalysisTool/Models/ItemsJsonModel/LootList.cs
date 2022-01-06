using Newtonsoft.Json;

namespace StatisticsAnalysisTool.Models.ItemWindowModel;

public class LootList
{
    [JsonProperty("loot")]
    public object Loot { get; set; }
}