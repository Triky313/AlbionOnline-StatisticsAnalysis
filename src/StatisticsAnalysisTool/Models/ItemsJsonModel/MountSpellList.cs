using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Models.ItemWindowModel;

public class MountSpellList
{
    [JsonPropertyName("mountspell")]
    public object MountSpell { get; set; }
}