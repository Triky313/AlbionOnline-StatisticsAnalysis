using Newtonsoft.Json;

namespace StatisticsAnalysisTool.Models.ItemWindowModel;

public class ConsumableFromInventoryItem
{
    [JsonProperty("@uniquename")]
    public string UniqueName { get; set; }

    [JsonProperty("@tradable")]
    public string Tradable { get; set; }

    [JsonProperty("@uisprite")]
    public string UiSprite { get; set; }

    [JsonProperty("@abilitypower")]
    public string AbilityPower { get; set; }

    [JsonProperty("@consumespell")]
    public string ConsumeSpell { get; set; }

    [JsonProperty("@ShopCategory")]
    public string ShopCategory { get; set; }

    [JsonProperty("@shopsubcategory1")]
    public string ShopSubCategory1 { get; set; }

    [JsonProperty("@tier")]
    public string Tier { get; set; }

    [JsonProperty("@weight")]
    public string Weight { get; set; }

    [JsonProperty("@dummyitempower")]
    public string DummyItemPower { get; set; }

    [JsonProperty("@maxstacksize")]
    public string MaxStackSize { get; set; }

    [JsonProperty("@unlockedtocraft")]
    public string UnlockedToCraft { get; set; }

    //[JsonProperty("@uicraftsoundstart")]
    //public string Uicraftsoundstart { get; set; }

    //[JsonProperty("@uicraftsoundfinish")]
    //public string Uicraftsoundfinish { get; set; }

    //[JsonProperty("@uispriteoverlay1")]
    //public string Uispriteoverlay1 { get; set; }

    [JsonProperty("@itemvalue")]
    public string ItemValue { get; set; }
    public CraftingRequirements CraftingRequirements { get; set; }

    [JsonProperty("@allowfullstackusage")]
    public string AllowFullStackUsage { get; set; }

    [JsonProperty("@logconsumption")]
    public string LogConsumption { get; set; }

    [JsonProperty("@enchantmentlevel")]
    public string EnchantmentLevel { get; set; }

    [JsonProperty("@descriptionlocatag")]
    public string DescriptionLocaTag { get; set; }

    [JsonProperty("@craftingcategory")]
    public string CraftingCategory { get; set; }
}