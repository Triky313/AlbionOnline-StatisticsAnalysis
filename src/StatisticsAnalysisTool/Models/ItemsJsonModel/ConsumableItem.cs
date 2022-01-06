using Newtonsoft.Json;

namespace StatisticsAnalysisTool.Models.ItemWindowModel;

public class ConsumableItem
{
    [JsonProperty("@uniquename")]
    public string UniqueName { get; set; }

    [JsonProperty("@fishingfame")]
    public string FishingFame { get; set; }

    [JsonProperty("@fishingminigamesetting")]
    public string FishingMiniGameSetting { get; set; }

    [JsonProperty("@descriptionlocatag")]
    public string DescriptionLocaTag { get; set; }

    [JsonProperty("@uisprite")]
    public string UiSprite { get; set; }

    [JsonProperty("@nutrition")]
    public string Nutrition { get; set; }

    [JsonProperty("@abilitypower")]
    public string AbilityPower { get; set; }

    [JsonProperty("@slottype")]
    public string SlotType { get; set; }

    [JsonProperty("@consumespell")]
    public string ConsumeSpell { get; set; }

    [JsonProperty("@ShopCategory")]
    public string ShopCategory { get; set; }

    [JsonProperty("@shopsubcategory1")]
    public string ShopSubCategory1 { get; set; }

    [JsonProperty("@resourcetype")]
    public string ResourceType { get; set; }

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

    [JsonProperty("@unlockedtoequip")]
    public string UnlockedToEquip { get; set; }

    [JsonProperty("@uicraftsoundstart")]
    public string UiCraftSoundStart { get; set; }

    [JsonProperty("@uicraftsoundfinish")]
    public string UiCraftSoundFinish { get; set; }

    [JsonProperty("@craftingcategory")]
    public string CraftingCategory { get; set; }
    public CraftingRequirements CraftingRequirements { get; set; }
    [JsonProperty("enchantments")]
    public Enchantments Enchantments { get; set; }
}