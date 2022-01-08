using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Models.ItemWindowModel;

public class UpgradeRequirements
{
    [JsonPropertyName("upgraderesource")]
    public UpgradeResource UpgradeResource { get; set; }
}