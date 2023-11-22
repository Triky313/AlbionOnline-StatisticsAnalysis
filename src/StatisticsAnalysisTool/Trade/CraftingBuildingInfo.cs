using System;

namespace StatisticsAnalysisTool.Trade;

public class CraftingBuildingInfo : IEquatable<CraftingBuildingInfo>
{
    public long? ObjectId { get; init; }
    public long BuildingObjectId { get; init; }
    public string BuildingName { get; init; }

    public bool Equals(CraftingBuildingInfo other)
    {
        if (other == null)
        {
            return false;
        }

        return ObjectId == other.ObjectId
               && BuildingObjectId == other.BuildingObjectId
               && BuildingName == other.BuildingName;
    }

    public override int GetHashCode()
    {
        int hash = 17;
        hash = hash * 23 + (ObjectId?.GetHashCode() ?? 0);
        hash = hash * 23 + BuildingObjectId.GetHashCode();
        hash = hash * 23 + (BuildingName?.GetHashCode() ?? 0);
        return hash;
    }
}