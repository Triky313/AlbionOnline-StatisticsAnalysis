using Newtonsoft.Json;

namespace StatisticsAnalysisTool.Models.ItemWindowModel;

public class AcceptedFood
{
    [JsonProperty("@foodcategory")]
    public string FoodCategory { get; set; }
}