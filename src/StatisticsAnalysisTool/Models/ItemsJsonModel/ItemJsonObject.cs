using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;

namespace StatisticsAnalysisTool.Models.ItemsJsonModel;

public abstract class ItemJsonObject
{
    public virtual string UniqueName { get; set; }
    public virtual ItemType ItemType { get; set; }
}