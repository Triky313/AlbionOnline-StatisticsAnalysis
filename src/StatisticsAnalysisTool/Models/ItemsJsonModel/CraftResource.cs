using Newtonsoft.Json;

namespace StatisticsAnalysisTool.Models.ItemWindowModel;

public class CraftResource
{
    [JsonProperty("@uniquename")]
    public string UniqueName { get; set; }

    [JsonProperty("@count")]
    public string Count { get; set; }

    [JsonProperty("@maxreturnamount")]
    public string MaxReturnAmount { get; set; }

    [JsonProperty("@enchantmentlevel")]
    public string EnchantmentLevel { get; set; }
}