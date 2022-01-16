using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Models.ItemsJsonModel;

public class CraftSpell
{
    [JsonPropertyName("@uniquename")]
    public string UniqueName { get; set; }
}