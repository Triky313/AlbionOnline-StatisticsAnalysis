using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Diagnostics;
using StatisticsAnalysisTool.Enumerations;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StatisticsAnalysisTool.Network.Events;

public class ActionOnBuildingFinishedEvent
{
    public long? UserObjectId;
    public readonly long BuildingObjectId;
    public readonly ActionOnBuildingType ActionType;

    public ActionOnBuildingFinishedEvent(Dictionary<byte, object> parameters)
    {
        try
        {
            if (parameters.TryGetValue(0, out object objectId))
            {
                UserObjectId = objectId.ObjectToLong();
            }

            if (parameters.TryGetValue(2, out object buildingObjectId))
            {
                BuildingObjectId = buildingObjectId.ObjectToLong() ?? -1;
            }

            if (parameters.TryGetValue(4, out object actionType))
            {
                var actionTypeNumber = actionType.ObjectToLong() ?? -1;
                ActionType = (ActionOnBuildingType) actionTypeNumber;
            }
        }
        catch (Exception e)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
        }
    }
}