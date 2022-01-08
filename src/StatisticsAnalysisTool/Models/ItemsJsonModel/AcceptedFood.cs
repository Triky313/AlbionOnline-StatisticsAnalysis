using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Models.ItemWindowModel;

public class AcceptedFood
{
    [JsonPropertyName("@foodcategory")]
    public string FoodCategory { get; set; }
}