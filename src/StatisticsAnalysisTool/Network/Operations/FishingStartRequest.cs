using Serilog;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Diagnostics;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StatisticsAnalysisTool.Network.Operations;

public class FishingStartRequest
{
    public readonly long EventId;
    public readonly int ItemIndex;

    public FishingStartRequest(Dictionary<byte, object> parameters)
    {
        try
        {
            if (parameters.TryGetValue(0, out object objectId))
            {
                EventId = objectId.ObjectToLong() ?? -1;
            }

            if (parameters.TryGetValue(2, out object itemIndex))
            {
                ItemIndex = itemIndex.ObjectToInt();
            }
        }
        catch (Exception e)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
        }
    }
}