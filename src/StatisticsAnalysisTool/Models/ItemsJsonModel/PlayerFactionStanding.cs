using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Models.ItemsJsonModel;

public class PlayerFactionStanding
{
    [JsonPropertyName("@faction")]
    public string Faction { get; set; }

    [JsonPropertyName("@minstanding")]
    public string MinStanding { get; set; }
}