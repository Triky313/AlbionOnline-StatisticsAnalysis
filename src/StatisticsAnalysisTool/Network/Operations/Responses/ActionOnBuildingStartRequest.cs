using Serilog;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StatisticsAnalysisTool.Network.Operations.Responses;

public class ActionOnBuildingStartRequest
{
    public readonly long Ticks;
    public readonly long BuildingObjectId;
    public readonly ActionOnBuildingType ActionType;
    public readonly long Costs;
    public readonly int ItemIndex;
    public readonly int Quantity;

    public ActionOnBuildingStartRequest(Dictionary<byte, object> parameters)
    {
        try
        {
            if (parameters.TryGetValue(0, out object timestamp))
            {
                Ticks = timestamp.ObjectToLong() ?? 0;
            }

            if (parameters.TryGetValue(1, out object buildingObjectId))
            {
                BuildingObjectId = buildingObjectId.ObjectToLong() ?? -1;
            }

            if (parameters.TryGetValue(2, out object actionType))
            {
                var actionTypeNumber = actionType.ObjectToLong() ?? -1;
                ActionType = (ActionOnBuildingType) actionTypeNumber;
            }

            if (parameters.TryGetValue(4, out object costs))
            {
                Costs = costs.ObjectToLong() ?? 0;
            }

            if (parameters.TryGetValue(7, out object itemIndex))
            {
                ItemIndex = itemIndex.ObjectToInt();
            }

            if (parameters.TryGetValue(9, out object quantity))
            {
                Quantity = quantity.ObjectToInt();
            }

        }
        catch (Exception e)
        {
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
        }
    }
}