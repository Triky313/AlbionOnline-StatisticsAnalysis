using Newtonsoft.Json;

namespace StatisticsAnalysisTool.Dungeon.Models;

public class MistsJsonObject
{
    [JsonProperty("@id")]
    public string Id { get; set; }
    
    [JsonProperty("@templatepool")]
    public string TemplatePool { get; set; }

    [JsonProperty("@clustertier")]
    public string ClusterTier { get; set; }

    [JsonProperty("@subbiome")]
    public string SubBiome { get; set; }
}