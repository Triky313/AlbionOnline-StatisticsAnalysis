using System.Collections.Generic;

namespace StatisticsAnalysisTool.Trade;

public class CraftingBuildingInfo : IEqualityComparer<CraftingBuildingInfo>
{
    public long? ObjectId { get; set; }
    public long BuildingObjectId { get; set; }
    public string BuildingName { get; set; }

    public bool Equals(CraftingBuildingInfo x, CraftingBuildingInfo y)
    {
        if (x == null || y == null)
        {
            return false;
        }

        return x.ObjectId == y.ObjectId
               && x.BuildingObjectId == y.BuildingObjectId
               && x.BuildingName == y.BuildingName;
    }

    public int GetHashCode(CraftingBuildingInfo obj)
    {
        int hash = 17;
        hash = hash * 23 + obj.ObjectId.GetHashCode();
        hash = hash * 23 + obj.BuildingObjectId.GetHashCode();
        hash = hash * 23 + obj.BuildingName.GetHashCode();
        return hash;
    }
}