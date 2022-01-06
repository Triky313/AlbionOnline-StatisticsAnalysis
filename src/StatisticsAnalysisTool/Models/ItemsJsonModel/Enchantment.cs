using Newtonsoft.Json;

namespace StatisticsAnalysisTool.Models.ItemWindowModel;

public class Enchantment
{
    [JsonProperty("@enchantmentlevel")]
    public string EnchantmentLevel { get; set; }

    [JsonProperty("@abilitypower")]
    public string AbilityPower { get; set; }

    [JsonProperty("@dummyitempower")]
    public string DummyItemPower { get; set; }

    [JsonProperty("@consumespell")]
    public string ConsumeSpell { get; set; }
    [JsonProperty("craftingrequirements")]
    public CraftingRequirements CraftingRequirements { get; set; }
    [JsonProperty("upgraderequirements")]
    public UpgradeRequirements UpgradeRequirements { get; set; }

    [JsonProperty("@itempower")]
    public string ItemPower { get; set; }

    [JsonProperty("@durability")]
    public string Durability { get; set; }
}