using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Models.ItemWindowModel;

public class Enchantments
{
    [JsonPropertyName("enchantment")]
    public object Enchantment { get; set; }
}