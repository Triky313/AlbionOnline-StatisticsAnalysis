using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Models.ItemsJsonModel;

public class ShopCategories
{
    [JsonPropertyName("shopcategory")]
    public List<ShopCategory> ShopCategory { get; set; }
}