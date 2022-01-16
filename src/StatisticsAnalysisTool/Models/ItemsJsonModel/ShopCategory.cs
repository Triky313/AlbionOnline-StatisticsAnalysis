using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Models.ItemsJsonModel;

public class ShopCategory
{
    [JsonPropertyName("@id")]
    public string Id { get; set; }

    [JsonPropertyName("@value")]
    public string Value { get; set; }
    [JsonPropertyName("shopsubcategory")]
    public object ShopSubCategory { get; set; }
}