using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.GameData.Models;

public class MobJsonRootObject
{
    [JsonPropertyName("Mobs")]
    public MobObjects Mobs { get; set; }
}