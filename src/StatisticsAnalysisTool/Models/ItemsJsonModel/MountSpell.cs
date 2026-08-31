using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Models.ItemsJsonModel;

public sealed class MountSpell
{
    [JsonPropertyName("@uniquename")]
    public string UniqueName { get; set; }

    [JsonPropertyName("@spellslot")]
    public string SpellSlot { get; set; }
}