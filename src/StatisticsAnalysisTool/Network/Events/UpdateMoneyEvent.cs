using Serilog;
using StatisticsAnalysisTool.Common;
using System;
using System.Collections.Generic;
using System.Reflection;
using StatisticsAnalysisTool.Diagnostics;

namespace StatisticsAnalysisTool.Network.Events;

public class UpdateMoneyEvent
{
    public UpdateMoneyEvent(Dictionary<byte, object> parameters)
    {
        try
        {
            if (parameters.ContainsKey(1))
            {
                CurrentPlayerSilver = FixPoint.FromInternalValue(parameters[1].ObjectToLong() ?? 0);
            }
        }
        catch (ArgumentNullException e)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
        }
    }

    public FixPoint CurrentPlayerSilver { get; }
}