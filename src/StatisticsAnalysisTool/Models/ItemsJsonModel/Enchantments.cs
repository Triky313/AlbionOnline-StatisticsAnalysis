using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Models.ItemsJsonModel;

public class Enchantments
{
    [JsonPropertyName("enchantment")]
    public object Enchantment { get; set; }
}