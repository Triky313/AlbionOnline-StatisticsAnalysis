using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Models.ItemWindowModel;

public class ConsumableFromInventoryItem
{
    [JsonPropertyName("@uniquename")]
    public string UniqueName { get; set; }

    [JsonPropertyName("@tradable")]
    public string Tradable { get; set; }

    [JsonPropertyName("@uisprite")]
    public string UiSprite { get; set; }

    [JsonPropertyName("@abilitypower")]
    public string AbilityPower { get; set; }

    [JsonPropertyName("@consumespell")]
    public string ConsumeSpell { get; set; }

    [JsonPropertyName("@ShopCategory")]
    public string ShopCategory { get; set; }

    [JsonPropertyName("@shopsubcategory1")]
    public string ShopSubCategory1 { get; set; }

    [JsonPropertyName("@tier")]
    public string Tier { get; set; }

    [JsonPropertyName("@weight")]
    public string Weight { get; set; }

    [JsonPropertyName("@dummyitempower")]
    public string DummyItemPower { get; set; }

    [JsonPropertyName("@maxstacksize")]
    public string MaxStackSize { get; set; }

    [JsonPropertyName("@unlockedtocraft")]
    public string UnlockedToCraft { get; set; }

    //[JsonPropertyName("@uicraftsoundstart")]
    //public string Uicraftsoundstart { get; set; }

    //[JsonPropertyName("@uicraftsoundfinish")]
    //public string Uicraftsoundfinish { get; set; }

    //[JsonPropertyName("@uispriteoverlay1")]
    //public string Uispriteoverlay1 { get; set; }

    [JsonPropertyName("@itemvalue")]
    public string ItemValue { get; set; }
    public CraftingRequirements CraftingRequirements { get; set; }

    [JsonPropertyName("@allowfullstackusage")]
    public string AllowFullStackUsage { get; set; }

    [JsonPropertyName("@logconsumption")]
    public string LogConsumption { get; set; }

    [JsonPropertyName("@enchantmentlevel")]
    public string EnchantmentLevel { get; set; }

    [JsonPropertyName("@descriptionlocatag")]
    public string DescriptionLocaTag { get; set; }

    [JsonPropertyName("@craftingcategory")]
    public string CraftingCategory { get; set; }
}