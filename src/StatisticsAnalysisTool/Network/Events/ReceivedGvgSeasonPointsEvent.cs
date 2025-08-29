using StatisticsAnalysisTool.Common;
using System;
using System.Collections.Generic;
using System.Reflection;
using StatisticsAnalysisTool.Diagnostics;

namespace StatisticsAnalysisTool.Network.Events;

public class ReceivedGvgSeasonPointsEvent
{
    public int SeasonPoints;

    public ReceivedGvgSeasonPointsEvent(Dictionary<byte, object> parameters)
    {
        try
        {
            if (parameters.ContainsKey(1))
            {
                SeasonPoints = parameters[1].ObjectToInt();
            }
        }
        catch (Exception e)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
        }
    }
}