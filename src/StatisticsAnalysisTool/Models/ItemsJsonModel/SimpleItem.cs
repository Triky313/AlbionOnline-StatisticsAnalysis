using StatisticsAnalysisTool.Common.Converters;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Models.ItemsJsonModel;

public class SimpleItem : ItemJsonObject
{
    [JsonPropertyName("@uniquename")]
    public override string UniqueName { get; set; }

    [JsonPropertyName("@tier")]
    public string Tier { get; set; }

    [JsonPropertyName("@weight")]
    public string Weight { get; set; }

    [JsonPropertyName("@maxstacksize")]
    public string MaxStackSize { get; set; }

    [JsonPropertyName("@uisprite")]
    public string UiSprite { get; set; }

    [JsonPropertyName("@shopcategory")]
    public override string ShopCategory { get; set; }

    [JsonPropertyName("@shopsubcategory1")]
    public override string ShopSubCategory1 { get; set; }

    [JsonPropertyName("@unlockedtocraft")]
    public bool UnlockedToCraft { get; set; }

    [JsonPropertyName("@itemvalue")]
    public string ItemValue { get; set; }

    [JsonPropertyName("@nutrition")]
    public string Nutrition { get; set; }

    [JsonPropertyName("@foodcategory")]
    public string FoodCategory { get; set; }

    [JsonPropertyName("@resourcetype")]
    public string ResourceType { get; set; }

    [JsonPropertyName("@famevalue")]
    public string FameValue { get; set; }

    [JsonPropertyName("@enchantmentlevel")]
    public string EnchantmentLevel { get; set; }

    [JsonPropertyName("@fishingfame")]
    public string FishingFame { get; set; }

    [JsonPropertyName("@fishingminigamesetting")]
    public string FishingMiniGameSetting { get; set; }

    [JsonConverter(typeof(CraftingRequirementsToCraftingRequirementsList))]
    [JsonPropertyName("craftingrequirements")]
    public List<CraftingRequirements> CraftingRequirements { get; set; }

    [JsonPropertyName("@descriptionlocatag")]
    public string DescriptionLocaTag { get; set; }

    [JsonPropertyName("@fasttravelfactor")]
    public string FastTravelFactor { get; set; }

    [JsonPropertyName("@shopsubcategory2")]
    public override string ShopSubCategory2 { get; set; }

    [JsonPropertyName("@shopsubcategory3")]
    public override string ShopSubCategory3 { get; set; }
}