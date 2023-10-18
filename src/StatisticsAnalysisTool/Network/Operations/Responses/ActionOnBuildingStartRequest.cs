using Serilog;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Models;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StatisticsAnalysisTool.Network.Operations.Responses;

public class ActionOnBuildingStartRequest
{
    public readonly long BuildingObjectId;
    public readonly long Costs;

    public ActionOnBuildingStartRequest(Dictionary<byte, object> parameters)
    {
        ConsoleManager.WriteLine(new ConsoleFragment(GetType().Name, parameters, ConsoleColorType.EventColor));

        try
        {
            if (parameters.TryGetValue(1, out object buildingObjectId))
            {
                BuildingObjectId = buildingObjectId.ObjectToLong() ?? -1;
            }

            if (parameters.TryGetValue(4, out object costs))
            {
                Costs = costs.ObjectToLong() ?? 0;
            }

        }
        catch (Exception e)
        {
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
        }
    }
}