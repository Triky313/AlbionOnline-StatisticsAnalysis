using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.GameFileData.Models;

public class MobJsonObject
{
    [JsonPropertyName("@uniquename")]
    public string UniqueName { get; set; }
    [JsonPropertyName("@tier")]
    public short Tier { get; set; }
    [JsonPropertyName("@hitpointsmax")]
    public double HitPointsMax { get; set; }
    [JsonPropertyName("@namelocatag")]
    public string NameLocatag { get; set; } = string.Empty;
    [JsonPropertyName("@avatar")]
    public string Avatar { get; set; } = string.Empty;
    [JsonPropertyName("@faction")]
    public string Faction { get; set; } = string.Empty;
}