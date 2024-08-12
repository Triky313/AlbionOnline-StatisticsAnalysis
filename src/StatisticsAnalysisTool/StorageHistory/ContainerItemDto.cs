using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.StorageHistory;

public class ContainerItemDto
{
    [JsonPropertyName("I")]
    public int ItemIndex { get; set; }
    [JsonPropertyName("Q")]
    public int Quantity { get; set; }
}