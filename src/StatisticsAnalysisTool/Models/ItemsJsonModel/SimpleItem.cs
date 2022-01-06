using Newtonsoft.Json;

namespace StatisticsAnalysisTool.Models.ItemWindowModel;

public class SimpleItem
{
    [JsonProperty("@uniquename")]
    public string UniqueName { get; set; }

    [JsonProperty("@tier")]
    public string Tier { get; set; }

    [JsonProperty("@weight")]
    public string Weight { get; set; }

    [JsonProperty("@maxstacksize")]
    public string MaxStackSize { get; set; }

    [JsonProperty("@uisprite")]
    public string UiSprite { get; set; }

    [JsonProperty("@ShopCategory")]
    public string ShopCategory { get; set; }

    [JsonProperty("@shopsubcategory1")]
    public string ShopSubCategory1 { get; set; }

    [JsonProperty("@unlockedtocraft")]
    public string UnlockedToCraft { get; set; }

    [JsonProperty("@itemvalue")]
    public string ItemValue { get; set; }

    [JsonProperty("@nutrition")]
    public string Nutrition { get; set; }

    [JsonProperty("@foodcategory")]
    public string FoodCategory { get; set; }

    [JsonProperty("@resourcetype")]
    public string ResourceType { get; set; }

    [JsonProperty("@famevalue")]
    public string FameValue { get; set; }

    [JsonProperty("@enchantmentlevel")]
    public string EnchantmentLevel { get; set; }

    [JsonProperty("@fishingfame")]
    public string FishingFame { get; set; }

    [JsonProperty("@fishingminigamesetting")]
    public string FishingMiniGameSetting { get; set; }
    [JsonProperty("craftingrequirements")]
    public object CraftingRequirements { get; set; }

    [JsonProperty("@descriptionlocatag")]
    public string DescriptionLocaTag { get; set; }

    [JsonProperty("@fasttravelfactor")]
    public string FastTravelFactor { get; set; }
}