using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Models.ItemWindowModel;

public class ShopCategories
{
    [JsonPropertyName("shopcategory")]
    public List<ShopCategory> ShopCategory { get; set; }
}