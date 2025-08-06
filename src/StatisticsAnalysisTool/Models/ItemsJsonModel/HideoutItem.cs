using StatisticsAnalysisTool.Common.Converters;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Models.ItemsJsonModel;

public class HideoutItem : ItemJsonObject
{
    [JsonPropertyName("@uniquename")]
    public override string UniqueName { get; set; }

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

    [JsonPropertyName("@shopcategory")]
    public override string ShopCategory { get; set; }

    [JsonPropertyName("@shopsubcategory1")]
    public override string ShopSubCategory1 { get; set; }

    //[JsonPropertyName("@uicraftsoundstart")]
    //public string UiCraftSoundStart { get; set; }

    //[JsonPropertyName("@uicraftsoundfinish")]
    //public string Uicraftsoundfinish { get; set; }

    [JsonConverter(typeof(CraftingRequirementsToCraftingRequirementsList))]
    [JsonPropertyName("craftingrequirements")]
    public List<CraftingRequirements> CraftingRequirements { get; set; }

    [JsonPropertyName("@shopsubcategory2")]
    public override string ShopSubCategory2 { get; set; }

    [JsonPropertyName("@shopsubcategory3")]
    public override string ShopSubCategory3 { get; set; }
}