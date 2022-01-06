using Newtonsoft.Json;

namespace StatisticsAnalysisTool.Models.ItemWindowModel;

public class CraftSpell
{
    [JsonProperty("@uniquename")]
    public string UniqueName { get; set; }
}