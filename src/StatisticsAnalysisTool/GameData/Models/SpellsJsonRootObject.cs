using System.Text.Json.Serialization;
using StatisticsAnalysisTool.Models;

namespace StatisticsAnalysisTool.GameData.Models;

public class SpellsJsonRootObject
{
    [JsonPropertyName("spells")]
    public ActiveSpellJsonObject SpellsJson { get; set; }
}