using System.Collections.Generic;
using System.Text.Json.Serialization;
using StatisticsAnalysisTool.Common.Converters;

namespace StatisticsAnalysisTool.Models.ItemsJsonModel;

public class FarmableItem : ItemJsonObject
{
    [JsonPropertyName("@uniquename")]
    public override string UniqueName { get; set; }

    [JsonPropertyName("@tier")]
    public string Tier { get; set; }

    [JsonPropertyName("@placefame")]
    public string PlaceFame { get; set; }

    [JsonPropertyName("@pickupable")]
    public bool Pickupable { get; set; }

    [JsonPropertyName("@destroyable")]
    public bool Destroyable { get; set; }

    [JsonPropertyName("@unlockedtoplace")]
    public bool UnlockedToPlace { get; set; }

    [JsonPropertyName("@maxstacksize")]
    public string MaxStackSize { get; set; }

    [JsonPropertyName("@shopcategory")]
    public override string ShopCategory { get; set; }

    [JsonPropertyName("@shopsubcategory1")]
    public override string ShopSubCategory1 { get; set; }

    [JsonPropertyName("@kind")]
    public string Kind { get; set; }

    [JsonPropertyName("@weight")]
    public string Weight { get; set; }

    [JsonPropertyName("@unlockedtocraft")]
    public bool UnlockedToCraft { get; set; }

    [JsonPropertyName("@animationid")]
    public string AnimationId { get; set; }

    [JsonPropertyName("@activefarmfocuscost")]
    public string ActiveFarmFocusCost { get; set; }

    [JsonPropertyName("@activefarmmaxcycles")]
    public string ActiveFarmMaxCycles { get; set; }

    [JsonPropertyName("@activefarmactiondurationseconds")]
    public string ActiveFarmActionDurationSeconds { get; set; }

    [JsonPropertyName("@activefarmcyclelengthseconds")]
    public string ActiveFarmCycleLengthSeconds { get; set; }

    [JsonPropertyName("@activefarmbonus")]
    public string ActiveFarmBonus { get; set; }

    [JsonPropertyName("@itemvalue")]
    public string ItemValue { get; set; }

    [JsonConverter(typeof(CraftingRequirementsToCraftingRequirementsList))]
    [JsonPropertyName("craftingrequirements")]
    public List<CraftingRequirements> CraftingRequirements { get; set; }
    //public AudioInfo AudioInfo { get; set; }
    [JsonPropertyName("harvest")]
    public Harvest Harvest { get; set; }

    [JsonPropertyName("@prefabname")]
    public string PrefabName { get; set; }

    [JsonPropertyName("@prefabscale")]
    public string PrefabScale { get; set; }

    [JsonPropertyName("@resourcevalue")]
    public string ResourceValue { get; set; }
    [JsonPropertyName("grownitem")]
    public GrownItem GrownItem { get; set; }
    [JsonPropertyName("consumption")]
    public Consumption Consumption { get; set; }

    [JsonPropertyName("@tile")]
    public string Tile { get; set; }

    [JsonPropertyName("@uisprite")]
    public string UiSprite { get; set; }

    [JsonPropertyName("@showinmarketplace")]
    public bool ShowInMarketPlace { get; set; }

    [JsonPropertyName("@shopsubcategory2")]
    public override string ShopSubCategory2 { get; set; }

    [JsonPropertyName("@shopsubcategory3")]
    public override string ShopSubCategory3 { get; set; }
}