using StatisticsAnalysisTool.Common.Converters;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Models.ItemsJsonModel;

public class Enchantment
{
    [JsonPropertyName("@enchantmentlevel")]
    public string EnchantmentLevel { get; set; }

    [JsonIgnore]
    public int EnchantmentLevelInteger => int.TryParse(EnchantmentLevel, out var enchantmentLevel) ? enchantmentLevel : 0;

    [JsonPropertyName("@abilitypower")]
    public string AbilityPower { get; set; }

    [JsonPropertyName("@dummyitempower")]
    public string DummyItemPower { get; set; }

    [JsonPropertyName("@consumespell")]
    public string ConsumeSpell { get; set; }

    [JsonPropertyName("@durability")]
    public double Durability { get; set; }

    [JsonConverter(typeof(CraftingRequirementsToCraftingRequirementsList))]
    [JsonPropertyName("craftingrequirements")]
    public List<CraftingRequirements> CraftingRequirements { get; set; }

    [JsonPropertyName("upgraderequirements")]
    public UpgradeRequirements UpgradeRequirements { get; set; }

    public void Reset()
    {
        EnchantmentLevel = null;
        AbilityPower = null;
        DummyItemPower = null;
        ConsumeSpell = null;
        Durability = 0;
        CraftingRequirements = null;
        UpgradeRequirements = null;
    }
}