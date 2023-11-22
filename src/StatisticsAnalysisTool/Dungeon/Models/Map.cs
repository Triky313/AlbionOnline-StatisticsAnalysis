using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Dungeon.Models;

public class Map
{
    [JsonPropertyName("@id")]
    public string Id { get; set; }

    [JsonPropertyName("@mastertemplate")]
    public string MasterTemplate { get; set; }

    [JsonPropertyName("@templatepool")]
    public string TemplatePool { get; set; }

    [JsonPropertyName("@clustertier")]
    public string ClusterTier { get; set; }

    [JsonPropertyName("@subbiome")]
    public string SubBiome { get; set; }

    [JsonPropertyName("@softplayerlimitmin")]
    public string SoftPlayerLimitMin { get; set; }

    [JsonPropertyName("@softplayerlimitmax")]
    public string SoftPlayerLimitMax { get; set; }

    [JsonPropertyName("@hardplayercapspace")]
    public string HardPlayerCapSpace { get; set; }

    [JsonPropertyName("@weight")]
    public string Weight { get; set; }

    [JsonPropertyName("@clusterclosetime")]
    public string ClusterCloseTime { get; set; }

    [JsonPropertyName("@clustershutdowntime")]
    public string ClusterShutdownTime { get; set; }

    [JsonPropertyName("@maxdifficulty")]
    public string MaxDifficulty { get; set; }

    [JsonPropertyName("@mindifficulty")]
    public string MinDifficulty { get; set; }

    [JsonPropertyName("@requiredentrancetier")]
    public string RequiredEntranceTier { get; set; }
}