using Newtonsoft.Json;

namespace StatisticsAnalysisTool.Models.ItemWindowModel;

public class MountSpellList
{
    [JsonProperty("mountspell")]
    public object MountSpell { get; set; }
}