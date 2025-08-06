using StatisticsAnalysisTool.Common.Converters;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Models.ItemsJsonModel;

public class ShopSubCategory2
{
    [JsonPropertyName("@id")]
    public string Id { get; set; }
    [JsonPropertyName("@value")]
    public string Value { get; set; }

    [JsonPropertyName("shopsubcategory3")]
    [JsonConverter(typeof(SingleOrArrayConverter<ShopSubCategory3>))]
    public List<ShopSubCategory3> ShopSubCategory3 { get; set; }
}