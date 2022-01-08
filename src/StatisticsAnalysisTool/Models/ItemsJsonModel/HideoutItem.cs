using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Models.ItemWindowModel;

public class HideoutItem
{
    [JsonPropertyName("@uniquename")]
    public string UniqueName { get; set; }

    [JsonPropertyName("@itemvalue")]
    public string ItemValue { get; set; }

    [JsonPropertyName("@tier")]
    public string Tier { get; set; }

    [JsonPropertyName("@mindistance")]
    public string MinDistance { get; set; }

    [JsonPropertyName("@mindistanceintunnel")]
    public string MinDistanceinTunnel { get; set; }

    [JsonPropertyName("@placementduration")]
    public string Placementduration { get; set; }

    [JsonPropertyName("@primetimedurationminutes")]
    public string Primetimedurationminutes { get; set; }

    [JsonPropertyName("@maxstacksize")]
    public string Maxstacksize { get; set; }

    [JsonPropertyName("@weight")]
    public string Weight { get; set; }

    [JsonPropertyName("@unlockedtocraft")]
    public string Unlockedtocraft { get; set; }

    [JsonPropertyName("@ShopCategory")]
    public string ShopCategory { get; set; }

    [JsonPropertyName("@shopsubcategory1")]
    public string ShopSubCategory1 { get; set; }

    //[JsonPropertyName("@uicraftsoundstart")]
    //public string UiCraftSoundStart { get; set; }

    //[JsonPropertyName("@uicraftsoundfinish")]
    //public string Uicraftsoundfinish { get; set; }
    [JsonPropertyName("craftingrequirements")]
    public CraftingRequirements CraftingRequirements { get; set; }
}