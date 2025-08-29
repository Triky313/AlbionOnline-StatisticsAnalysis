using StatisticsAnalysisTool.Common;
using System;
using System.Collections.Generic;
using System.Reflection;
using StatisticsAnalysisTool.Diagnostics;

namespace StatisticsAnalysisTool.Network.Events;

public class LeaveEvent
{
    public LeaveEvent(Dictionary<byte, object> parameters)
    {
        try
        {
            if (parameters.ContainsKey(0)) ObjectId = parameters[0].ObjectToLong();
        }
        catch (Exception e)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
        }
    }

    public long? ObjectId { get; }
}