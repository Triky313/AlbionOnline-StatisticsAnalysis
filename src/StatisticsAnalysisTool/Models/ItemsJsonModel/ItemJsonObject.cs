using StatisticsAnalysisTool.Enumerations;

namespace StatisticsAnalysisTool.Models.ItemsJsonModel;

public abstract class ItemJsonObject
{
    public virtual string UniqueName { get; set; }
    public virtual ItemType ItemType { get; set; }

    public virtual string ShopCategory { get; set; }
    public virtual string ShopSubCategory1 { get; set; }
    public virtual string ShopSubCategory2 { get; set; }
    public virtual string ShopSubCategory3 { get; set; }
}