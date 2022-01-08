using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Models.ItemWindowModel;

public class Enchantment
{
    [JsonPropertyName("@enchantmentlevel")]
    public string EnchantmentLevel { get; set; }

    [JsonPropertyName("@abilitypower")]
    public string AbilityPower { get; set; }

    [JsonPropertyName("@dummyitempower")]
    public string DummyItemPower { get; set; }

    [JsonPropertyName("@consumespell")]
    public string ConsumeSpell { get; set; }
    [JsonPropertyName("craftingrequirements")]
    public CraftingRequirements CraftingRequirements { get; set; }
    [JsonPropertyName("upgraderequirements")]
    public UpgradeRequirements UpgradeRequirements { get; set; }

    [JsonPropertyName("@itempower")]
    public string ItemPower { get; set; }

    [JsonPropertyName("@durability")]
    public string Durability { get; set; }
}