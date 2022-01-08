using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Models.ItemWindowModel;

public class LabourerContract
{
    [JsonPropertyName("@uniquename")]
    public string UniqueName { get; set; }

    [JsonPropertyName("@tier")]
    public string Tier { get; set; }

    [JsonPropertyName("@ShopCategory")]
    public string ShopCategory { get; set; }

    [JsonPropertyName("@shopsubcategory1")]
    public string ShopSubCategory1 { get; set; }

    [JsonPropertyName("@unlockedtocraft")]
    public string UnlockedToCraft { get; set; }

    [JsonPropertyName("@weight")]
    public string Weight { get; set; }
}