using Newtonsoft.Json;

namespace StatisticsAnalysisTool.Models.ItemWindowModel;

public class Enchantments
{
    [JsonProperty("enchantment")]
    public Enchantment Enchantment { get; set; }
}