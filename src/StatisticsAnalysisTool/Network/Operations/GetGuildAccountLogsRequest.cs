using Serilog;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Diagnostics;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StatisticsAnalysisTool.Network.Operations;

public class GetGuildAccountLogsRequest
{
    public int TabId { get; set; }

    public GetGuildAccountLogsRequest(Dictionary<byte, object> parameters)
    {
        try
        {
            if (parameters.TryGetValue(1, out object objectId))
            {
                TabId = objectId.ObjectToInt();
            }
        }
        catch (Exception e)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
        }
    }
}