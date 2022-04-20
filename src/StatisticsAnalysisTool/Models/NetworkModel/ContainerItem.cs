using System.Text.Json.Serialization;
using StatisticsAnalysisTool.Common;

namespace StatisticsAnalysisTool.Models.NetworkModel;

public class ContainerItem
{
    public int ItemIndex { get; set; }
    [JsonIgnore]
    public Item Item => ItemController.GetItemByIndex(ItemIndex);
    public int Quantity { get; set; }
}