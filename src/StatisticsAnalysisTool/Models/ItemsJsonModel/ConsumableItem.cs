using System.Collections.Generic;
using System.Text.Json.Serialization;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Common.Converters;
using StatisticsAnalysisTool.Enumerations;

namespace StatisticsAnalysisTool.Models.ItemsJsonModel;

public class ConsumableItem : ItemJsonObject
{
    [JsonPropertyName("@uniquename")]
    public override string UniqueName { get; set; }

    [JsonPropertyName("@itemvalue")]
    public string ItemValue { get; set; }

    [JsonPropertyName("@fishingfame")]
    public string FishingFame { get; set; }

    [JsonPropertyName("@fishingminigamesetting")]
    public string FishingMiniGameSetting { get; set; }

    [JsonPropertyName("@descriptionlocatag")]
    public string DescriptionLocaTag { get; set; }

    [JsonPropertyName("@uisprite")]
    public string UiSprite { get; set; }

    [JsonPropertyName("@nutrition")]
    public string Nutrition { get; set; }

    [JsonPropertyName("@abilitypower")]
    public string AbilityPower { get; set; }

    [JsonPropertyName("@slottype")]
    public string SlotType { get; set; }
    [JsonIgnore]
    public SlotType SlotTypeEnum => ItemController.GetSlotType(SlotType);

    [JsonPropertyName("@consumespell")]
    public string ConsumeSpell { get; set; }

    [JsonPropertyName("@shopcategory")]
    public string ShopCategory { get; set; }

    [JsonPropertyName("@shopsubcategory1")]
    public string ShopSubCategory1 { get; set; }

    [JsonPropertyName("@resourcetype")]
    public string ResourceType { get; set; }

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

    [JsonPropertyName("@unlockedtoequip")]
    public string UnlockedToEquip { get; set; }

    //[JsonPropertyName("@uicraftsoundstart")]
    //public string UiCraftSoundStart { get; set; }

    //[JsonPropertyName("@uicraftsoundfinish")]
    //public string UiCraftSoundFinish { get; set; }

    [JsonPropertyName("@craftingcategory")]
    public string CraftingCategory { get; set; }

    [JsonConverter(typeof(CraftingRequirementsToCraftingRequirementsList))]
    [JsonPropertyName("craftingrequirements")]
    public List<CraftingRequirements> CraftingRequirements { get; set; }

    [JsonPropertyName("enchantments")]
    public Enchantments Enchantments { get; set; }
}