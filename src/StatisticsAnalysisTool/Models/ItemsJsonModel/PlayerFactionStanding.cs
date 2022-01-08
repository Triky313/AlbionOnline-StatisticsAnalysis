using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Models.ItemWindowModel;

public class PlayerFactionStanding
{
    [JsonPropertyName("@faction")]
    public string Faction { get; set; }

    [JsonPropertyName("@minstanding")]
    public string MinStanding { get; set; }
}