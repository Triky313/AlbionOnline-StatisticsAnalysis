using StatisticsAnalysisTool.GameFileData.Models;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Models;

public class ActiveSpellJsonObject
{
    [JsonPropertyName("activespell")]
    public List<SpellsJsonObject> ActiveSpells { get; set; }
}