using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Models;

public class SpellsJsonRootObject
{
    [JsonPropertyName("spells")]
    public ActiveSpellJsonObject SpellsJson { get; set; }
}