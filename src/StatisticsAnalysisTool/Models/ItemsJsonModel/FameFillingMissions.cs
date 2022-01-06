using Newtonsoft.Json;

namespace StatisticsAnalysisTool.Models.ItemWindowModel;

public class FameFillingMissions
{
    [JsonProperty("gatherfame")]
    public GatherFame GatherFame { get; set; }
    [JsonProperty("craftitemfame")]
    public CraftItemFame CraftItemFame { get; set; }
}