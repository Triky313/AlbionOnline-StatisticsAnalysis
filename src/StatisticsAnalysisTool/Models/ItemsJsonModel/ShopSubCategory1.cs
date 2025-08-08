using StatisticsAnalysisTool.Common.Converters;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Models.ItemsJsonModel;

public class ShopSubCategory1
{
    [JsonPropertyName("@id")]
    public string Id { get; set; }
    [JsonPropertyName("@value")]
    public string Value { get; set; }

    [JsonPropertyName("shopsubcategory2")]
    [JsonConverter(typeof(SingleOrArrayConverter<ShopSubCategory2>))]
    public List<ShopSubCategory2> ShopSubCategory2 { get; set; }
}