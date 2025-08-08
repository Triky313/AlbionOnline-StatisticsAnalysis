using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Models.ItemsJsonModel;

public class LabourerContract : ItemJsonObject
{
    [JsonPropertyName("@uniquename")]
    public override string UniqueName { get; set; }

    [JsonPropertyName("@tier")]
    public string Tier { get; set; }

    [JsonPropertyName("@shopcategory")]
    public override string ShopCategory { get; set; }

    [JsonPropertyName("@shopsubcategory1")]
    public override string ShopSubCategory1 { get; set; }

    [JsonPropertyName("@unlockedtocraft")]
    public string UnlockedToCraft { get; set; }

    [JsonPropertyName("@weight")]
    public string Weight { get; set; }

    [JsonPropertyName("@shopsubcategory2")]
    public override string ShopSubCategory2 { get; set; }

    [JsonPropertyName("@shopsubcategory3")]
    public override string ShopSubCategory3 { get; set; }
}