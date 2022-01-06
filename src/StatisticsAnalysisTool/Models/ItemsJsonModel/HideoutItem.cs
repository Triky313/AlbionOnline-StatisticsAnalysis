using Newtonsoft.Json;

namespace StatisticsAnalysisTool.Models.ItemWindowModel;

public class HideoutItem
{
    [JsonProperty("@uniquename")]
    public string UniqueName { get; set; }

    [JsonProperty("@itemvalue")]
    public string ItemValue { get; set; }

    [JsonProperty("@tier")]
    public string Tier { get; set; }

    [JsonProperty("@mindistance")]
    public string MinDistance { get; set; }

    [JsonProperty("@mindistanceintunnel")]
    public string MinDistanceinTunnel { get; set; }

    [JsonProperty("@placementduration")]
    public string Placementduration { get; set; }

    [JsonProperty("@primetimedurationminutes")]
    public string Primetimedurationminutes { get; set; }

    [JsonProperty("@maxstacksize")]
    public string Maxstacksize { get; set; }

    [JsonProperty("@weight")]
    public string Weight { get; set; }

    [JsonProperty("@unlockedtocraft")]
    public string Unlockedtocraft { get; set; }

    [JsonProperty("@ShopCategory")]
    public string ShopCategory { get; set; }

    [JsonProperty("@shopsubcategory1")]
    public string ShopSubCategory1 { get; set; }

    //[JsonProperty("@uicraftsoundstart")]
    //public string UiCraftSoundStart { get; set; }

    //[JsonProperty("@uicraftsoundfinish")]
    //public string Uicraftsoundfinish { get; set; }
    [JsonProperty("craftingrequirements")]
    public CraftingRequirements CraftingRequirements { get; set; }
}