using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Models.ItemsJsonModel;

public class CraftingSpellList
{
    [JsonPropertyName("@reference")]
    public string Reference { get; set; }

    [JsonConverter(typeof(Common.Converters.SingleOrArrayConverter<CraftSpell>))]
    [JsonPropertyName("craftspell")]
    public System.Collections.Generic.List<CraftSpell> CraftSpells { get; set; }
}