using System;
using System.Collections.Generic;
using System.Reflection;
using StatisticsAnalysisTool.Common;

namespace StatisticsAnalysisTool.Network.Events;

public class ReceivedSeasonPointsEvent : BaseEvent
{
    public int SeasonPoints;

    public ReceivedSeasonPointsEvent(Dictionary<byte, object> parameters) : base(parameters)
    {
        ConsoleManager.WriteLineForNetworkHandler(GetType().Name, parameters);

        try
        {
            if (parameters.ContainsKey(1))
            {
                SeasonPoints = parameters[1].ObjectToInt();
            }
        }
        catch (Exception e)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
        }
    }
}