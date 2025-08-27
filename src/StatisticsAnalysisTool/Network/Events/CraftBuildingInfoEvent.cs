using StatisticsAnalysisTool.Common;
using System;
using System.Collections.Generic;
using System.Reflection;
using StatisticsAnalysisTool.Diagnostics;

namespace StatisticsAnalysisTool.Network.Events;

public class CraftBuildingInfoEvent
{
    public long? ObjectId;
    public readonly long BuildingObjectId;
    public readonly string BuildingName;

    public CraftBuildingInfoEvent(Dictionary<byte, object> parameters)
    {
        try
        {
            if (parameters.TryGetValue(0, out object objectId))
            {
                ObjectId = objectId.ObjectToLong();
            }

            if (parameters.TryGetValue(2, out object buildingObjectId))
            {
                BuildingObjectId = buildingObjectId.ObjectToLong() ?? -1;
            }

            if (parameters.TryGetValue(3, out object buildingName))
            {
                BuildingName = buildingName.ToString();
            }
        }
        catch (Exception e)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
        }
    }
}