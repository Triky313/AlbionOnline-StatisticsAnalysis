using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.GameFileData.Models;

public class MobJsonRootObject
{
    [JsonPropertyName("Mobs")]
    public MobObjects Mobs { get; set; }
}