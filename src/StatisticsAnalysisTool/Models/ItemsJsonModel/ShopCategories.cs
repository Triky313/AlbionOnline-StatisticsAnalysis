using System.Collections.Generic;
using Newtonsoft.Json;

namespace StatisticsAnalysisTool.Models.ItemWindowModel;

public class ShopCategories
{
    [JsonProperty("shopcategory")]
    public List<ShopCategory> ShopCategory { get; set; }
}