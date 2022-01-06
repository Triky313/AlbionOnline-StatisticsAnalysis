using Newtonsoft.Json;

namespace StatisticsAnalysisTool.Models.ItemWindowModel;

public class UpgradeRequirements
{
    [JsonProperty("upgraderesource")]
    public UpgradeResource UpgradeResource { get; set; }
}