using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Models.ItemWindowModel;

public class FameFillingMissions
{
    [JsonPropertyName("gatherfame")]
    public GatherFame GatherFame { get; set; }
    [JsonPropertyName("craftitemfame")]
    public CraftItemFame CraftItemFame { get; set; }
}