using Serilog;
using StatisticsAnalysisTool.Common;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StatisticsAnalysisTool.Network.Events;

public sealed class RerollItemTraitValueFinishedEvent
{
    public long UserObjectId { get; } = -1;
    public long BuildingObjectId { get; } = -1;
    public bool IsProc { get; }

    public RerollItemTraitValueFinishedEvent(Dictionary<byte, object> parameters)
    {
        try
        {
            if (parameters.TryGetValue(0, out object userObjectId))
            {
                UserObjectId = userObjectId.ObjectToLong() ?? -1;
            }

            if (parameters.TryGetValue(2, out object buildingObjectId))
            {
                BuildingObjectId = buildingObjectId.ObjectToLong() ?? -1;
            }

            if (parameters.TryGetValue(6, out object isProc))
            {
                IsProc = isProc.ObjectToBool();
            }
        }
        catch (Exception e)
        {
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
        }
    }
}