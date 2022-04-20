using System.Text.Json.Serialization;
using StatisticsAnalysisTool.Common;

namespace StatisticsAnalysisTool.Models.NetworkModel;

public class ContainerItem
{
    [JsonPropertyName("I")]
    public int ItemIndex { get; set; }
    [JsonIgnore]
    public Item Item => ItemController.GetItemByIndex(ItemIndex);
    [JsonPropertyName("Q")]
    public int Quantity { get; set; }
}