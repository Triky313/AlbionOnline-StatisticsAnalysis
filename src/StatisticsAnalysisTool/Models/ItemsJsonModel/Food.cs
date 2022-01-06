using Newtonsoft.Json;

namespace StatisticsAnalysisTool.Models.ItemWindowModel;

public class Food
{
    [JsonProperty("@nutritionmax")]
    public string NutritionMax { get; set; }

    [JsonProperty("@secondspernutrition")]
    public string SecondSpernutrition { get; set; }

    [JsonProperty("acceptedfood")]
    public AcceptedFood AcceptedFood { get; set; }

    [JsonProperty("@lossbeforehungry")]
    public string LossBeforeHungry { get; set; }
}