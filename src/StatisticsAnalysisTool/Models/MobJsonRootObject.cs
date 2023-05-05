using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Models;

public class MobJsonRootObject
{
    [JsonPropertyName("Mobs")]
    public MobObjects Mobs { get; set; }
}