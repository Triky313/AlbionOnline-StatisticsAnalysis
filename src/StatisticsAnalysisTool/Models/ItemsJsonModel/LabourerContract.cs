using Newtonsoft.Json;

namespace StatisticsAnalysisTool.Models.ItemWindowModel;

public class LabourerContract
{
    [JsonProperty("@uniquename")]
    public string UniqueName { get; set; }

    [JsonProperty("@tier")]
    public string Tier { get; set; }

    [JsonProperty("@ShopCategory")]
    public string ShopCategory { get; set; }

    [JsonProperty("@shopsubcategory1")]
    public string ShopSubCategory1 { get; set; }

    [JsonProperty("@unlockedtocraft")]
    public string UnlockedToCraft { get; set; }

    [JsonProperty("@weight")]
    public string Weight { get; set; }
}