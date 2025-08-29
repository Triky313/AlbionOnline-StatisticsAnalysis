using StatisticsAnalysisTool.Common;
using System;
using System.Collections.Generic;
using System.Reflection;
using StatisticsAnalysisTool.Diagnostics;

namespace StatisticsAnalysisTool.Network.Events;

public class PartyPlayerLeftEvent
{
    public Guid? UserGuid;

    public PartyPlayerLeftEvent(Dictionary<byte, object> parameters)
    {
        try
        {
            if (parameters.ContainsKey(1))
            {
                UserGuid = parameters[1].ObjectToGuid();
            }
        }
        catch (Exception e)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
        }
    }
}