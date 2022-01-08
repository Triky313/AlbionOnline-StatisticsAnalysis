using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Models.ItemWindowModel;

public class CraftSpell
{
    [JsonPropertyName("@uniquename")]
    public string UniqueName { get; set; }
}