using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Models.ItemsJsonModel;

public class AcceptedFood
{
    [JsonPropertyName("@foodcategory")]
    public string FoodCategory { get; set; }
}