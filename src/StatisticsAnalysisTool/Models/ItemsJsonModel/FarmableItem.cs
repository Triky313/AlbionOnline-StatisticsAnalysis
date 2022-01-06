using Newtonsoft.Json;

namespace StatisticsAnalysisTool.Models.ItemWindowModel;

public class FarmableItem
{
    [JsonProperty("@uniquename")]
    public string UniqueName { get; set; }

    [JsonProperty("@tier")]
    public string Tier { get; set; }

    [JsonProperty("@placefame")]
    public string PlaceFame { get; set; }

    [JsonProperty("@pickupable")]
    public string Pickupable { get; set; }

    [JsonProperty("@destroyable")]
    public string Destroyable { get; set; }

    [JsonProperty("@unlockedtoplace")]
    public string UnlockedToPlace { get; set; }

    [JsonProperty("@maxstacksize")]
    public string MaxStackSize { get; set; }

    [JsonProperty("@ShopCategory")]
    public string ShopCategory { get; set; }

    [JsonProperty("@shopsubcategory1")]
    public string ShopSubCategory1 { get; set; }

    [JsonProperty("@kind")]
    public string Kind { get; set; }

    [JsonProperty("@weight")]
    public string Weight { get; set; }

    [JsonProperty("@unlockedtocraft")]
    public string UnlockedToCraft { get; set; }

    [JsonProperty("@animationid")]
    public string AnimationId { get; set; }

    [JsonProperty("@activefarmfocuscost")]
    public string ActiveFarmFocusCost { get; set; }

    [JsonProperty("@activefarmmaxcycles")]
    public string ActiveFarmMaxCycles { get; set; }

    [JsonProperty("@activefarmactiondurationseconds")]
    public string ActiveFarmActionDurationSeconds { get; set; }

    [JsonProperty("@activefarmcyclelengthseconds")]
    public string ActiveFarmCycleLengthSeconds { get; set; }

    [JsonProperty("@activefarmbonus")]
    public string ActiveFarmBonus { get; set; }

    [JsonProperty("@itemvalue")]
    public string ItemValue { get; set; }
    public CraftingRequirements CraftingRequirements { get; set; }
    //public AudioInfo AudioInfo { get; set; }
    [JsonProperty("harvest")]
    public Harvest Harvest { get; set; }

    [JsonProperty("@prefabname")]
    public string PrefabName { get; set; }

    [JsonProperty("@prefabscale")]
    public string PrefabScale { get; set; }

    [JsonProperty("@resourcevalue")]
    public string ResourceValue { get; set; }
    [JsonProperty("grownitem")]
    public GrownItem GrownItem { get; set; }
    [JsonProperty("consumption")]
    public Consumption Consumption { get; set; }

    [JsonProperty("@tile")]
    public string Tile { get; set; }

    [JsonProperty("@uisprite")]
    public string UiSprite { get; set; }

    [JsonProperty("@showinmarketplace")]
    public string ShowInMarketPlace { get; set; }
}