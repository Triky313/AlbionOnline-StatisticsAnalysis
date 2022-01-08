using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Models.ItemWindowModel;

public class CraftResource
{
    [JsonPropertyName("@uniquename")]
    public string UniqueName { get; set; }

    [JsonPropertyName("@count")]
    public string Count { get; set; }

    [JsonPropertyName("@maxreturnamount")]
    public string MaxReturnAmount { get; set; }

    [JsonPropertyName("@enchantmentlevel")]
    public string EnchantmentLevel { get; set; }
}