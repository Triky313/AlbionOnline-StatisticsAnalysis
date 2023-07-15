using System.Collections.Generic;
using System.Text.Json.Serialization;
using StatisticsAnalysisTool.GameData.Models;

namespace StatisticsAnalysisTool.Models;

public class ActiveSpellJsonObject
{
    [JsonPropertyName("activespell")]
    public List<SpellsJsonObject> ActiveSpells { get; set; }
}