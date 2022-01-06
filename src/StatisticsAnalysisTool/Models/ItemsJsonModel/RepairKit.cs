using Newtonsoft.Json;

namespace StatisticsAnalysisTool.Models.ItemWindowModel;

public class RepairKit
{
    [JsonProperty("@repaircostfactor")]
    public string RepairCostFactor { get; set; }

    [JsonProperty("@maxtier")]
    public string MaxTier { get; set; }
}