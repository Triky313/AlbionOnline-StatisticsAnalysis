using StatisticsAnalysisTool.Common;
using System;
using System.Collections.Generic;
using System.Reflection;
using Serilog;

namespace StatisticsAnalysisTool.Network.Operations;

public class GetGuildAccountLogsRequest
{
    public int TabId { get; set; }

    public GetGuildAccountLogsRequest(Dictionary<byte, object> parameters)
    {
        ConsoleManager.WriteLineForNetworkHandler(GetType().Name, parameters);

        try
        {
            if (parameters.TryGetValue(1, out object objectId))
            {
                TabId = objectId.ObjectToInt();
            }
        }
        catch (Exception e)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
        }
    }
}