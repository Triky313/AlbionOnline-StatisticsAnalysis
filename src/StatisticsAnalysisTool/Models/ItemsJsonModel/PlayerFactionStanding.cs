using Newtonsoft.Json;

namespace StatisticsAnalysisTool.Models.ItemWindowModel;

public class PlayerFactionStanding
{
    [JsonProperty("@faction")]
    public string Faction { get; set; }

    [JsonProperty("@minstanding")]
    public string MinStanding { get; set; }
}