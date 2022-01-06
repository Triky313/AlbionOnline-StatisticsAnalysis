using Newtonsoft.Json;

namespace StatisticsAnalysisTool.Models.ItemWindowModel;

public class ShopCategory
{
    [JsonProperty("@id")]
    public string Id { get; set; }

    [JsonProperty("@value")]
    public string Value { get; set; }
    [JsonProperty("shopsubcategory")]
    public object ShopSubCategory { get; set; }
}