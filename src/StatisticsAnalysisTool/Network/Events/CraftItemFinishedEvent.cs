using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Diagnostics;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StatisticsAnalysisTool.Network.Events;

public class CraftItemFinishedEvent
{
    public readonly long? UserObjectId;
    public readonly long BuildingObjectId = -1;

    public CraftItemFinishedEvent(Dictionary<byte, object> parameters)
    {
        try
        {
            if (parameters.TryGetValue(0, out object userObjectId))
            {
                UserObjectId = userObjectId.ObjectToLong();
            }

            if (parameters.TryGetValue(1, out object buildingObjectId))
            {
                BuildingObjectId = buildingObjectId.ObjectToLong() ?? -1;
            }
        }
        catch (Exception e)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
        }
    }
}