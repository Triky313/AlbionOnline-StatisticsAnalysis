using Newtonsoft.Json;

namespace StatisticsAnalysisTool.Models.ItemWindowModel;

public class CraftingSpellList
{
    [JsonProperty("craftspell")]
    public CraftSpell CraftSpell { get; set; }
}