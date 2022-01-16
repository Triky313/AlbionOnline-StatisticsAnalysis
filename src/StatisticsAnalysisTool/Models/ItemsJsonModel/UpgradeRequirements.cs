using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Models.ItemsJsonModel;

public class UpgradeRequirements
{
    [JsonPropertyName("upgraderesource")]
    public UpgradeResource UpgradeResource { get; set; }
}