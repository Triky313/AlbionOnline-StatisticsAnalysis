using StatisticsAnalysisTool.Models;
using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.GameFileData.Models;

public class SpellsJsonRootObject
{
    [JsonPropertyName("spells")]
    public ActiveSpellJsonObject SpellsJson { get; set; }
}