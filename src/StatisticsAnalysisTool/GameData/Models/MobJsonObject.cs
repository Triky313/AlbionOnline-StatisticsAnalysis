using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.GameData.Models;

public class MobJsonObject
{
    [JsonPropertyName("@uniquename")]
    public string UniqueName { get; set; }
    [JsonPropertyName("@tier")]
    public short Tier { get; set; }
    [JsonPropertyName("@hitpointsmax")]
    public double HitPointsMax { get; set; }
}