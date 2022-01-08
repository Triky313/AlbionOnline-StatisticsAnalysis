using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Models.ItemWindowModel;

public class FarmableItem
{
    [JsonPropertyName("@uniquename")]
    public string UniqueName { get; set; }

    [JsonPropertyName("@tier")]
    public string Tier { get; set; }

    [JsonPropertyName("@placefame")]
    public string PlaceFame { get; set; }

    [JsonPropertyName("@pickupable")]
    public string Pickupable { get; set; }

    [JsonPropertyName("@destroyable")]
    public string Destroyable { get; set; }

    [JsonPropertyName("@unlockedtoplace")]
    public string UnlockedToPlace { get; set; }

    [JsonPropertyName("@maxstacksize")]
    public string MaxStackSize { get; set; }

    [JsonPropertyName("@ShopCategory")]
    public string ShopCategory { get; set; }

    [JsonPropertyName("@shopsubcategory1")]
    public string ShopSubCategory1 { get; set; }

    [JsonPropertyName("@kind")]
    public string Kind { get; set; }

    [JsonPropertyName("@weight")]
    public string Weight { get; set; }

    [JsonPropertyName("@unlockedtocraft")]
    public string UnlockedToCraft { get; set; }

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
    public CraftingRequirements CraftingRequirements { get; set; }
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
    public string ShowInMarketPlace { get; set; }
}