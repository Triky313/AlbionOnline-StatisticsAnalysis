using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Models.ItemsJsonModel;

public class RepairKit
{
    [JsonPropertyName("@repaircostfactor")]
    public string RepairCostFactor { get; set; }

    [JsonPropertyName("@maxtier")]
    public string MaxTier { get; set; }
}