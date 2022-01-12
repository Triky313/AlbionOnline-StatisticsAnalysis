using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Models.ItemsJsonModel;

public class Food
{
    [JsonPropertyName("@nutritionmax")]
    public string NutritionMax { get; set; }

    [JsonPropertyName("@secondspernutrition")]
    public string SecondSpernutrition { get; set; }

    [JsonPropertyName("acceptedfood")]
    public AcceptedFood AcceptedFood { get; set; }

    [JsonPropertyName("@lossbeforehungry")]
    public string LossBeforeHungry { get; set; }
}