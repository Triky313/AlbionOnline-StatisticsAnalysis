using Serilog;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Diagnostics;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StatisticsAnalysisTool.Network.Operations;

public class FishingFinishRequest
{
    public bool Succeeded { get; set; }

    public FishingFinishRequest(Dictionary<byte, object> parameters)
    {
        try
        {
            if (parameters.TryGetValue(1, out object objectId))
            {
                Succeeded = objectId.ObjectToBool();
            }
        }
        catch (Exception e)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
        }
    }
}