using StatisticsAnalysisTool.GameFileData.Models;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Models;

public class SpellsJson
{
    [JsonPropertyName("passivespell")]
    public List<SpellsJsonObject> PassiveSpells { get; set; }

    [JsonPropertyName("activespell")]
    public List<SpellsJsonObject> ActiveSpells { get; set; }

    [JsonPropertyName("togglespell")]
    public List<SpellsJsonObject> ToggleSpells { get; set; }
}