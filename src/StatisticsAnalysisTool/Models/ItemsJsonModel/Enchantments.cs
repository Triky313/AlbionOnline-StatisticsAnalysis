using StatisticsAnalysisTool.Common.Converters;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Models.ItemsJsonModel;

public class Enchantments
{
    [JsonConverter(typeof(EnchantmentToEnchantmentList))]
    [JsonPropertyName("enchantment")]
    public List<Enchantment> Enchantment { get; set; }
}