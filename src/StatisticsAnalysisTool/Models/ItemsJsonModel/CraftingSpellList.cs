using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Models.ItemWindowModel;

public class CraftingSpellList
{
    [JsonPropertyName("craftspell")]
    public object CraftSpell { get; set; }
}