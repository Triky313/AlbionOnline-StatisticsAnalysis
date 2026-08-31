using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Models.ItemsJsonModel;

public class MountSpellList
{
    [JsonConverter(typeof(Common.Converters.SingleOrArrayConverter<MountSpell>))]
    [JsonPropertyName("mountspell")]
    public System.Collections.Generic.List<MountSpell> MountSpells { get; set; }
}