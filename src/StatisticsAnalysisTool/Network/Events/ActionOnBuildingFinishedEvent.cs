using System;
using System.Collections.Generic;
using System.Reflection;
using StatisticsAnalysisTool.Common;

namespace StatisticsAnalysisTool.Network.Events;

public class ActionOnBuildingFinishedEvent
{
    public long? UserObjectId;
    public long BuildingObjectId;

    public ActionOnBuildingFinishedEvent(Dictionary<byte, object> parameters)
    {
        ConsoleManager.WriteLineForNetworkHandler(GetType().Name, parameters);

        try
        {
            if (parameters.ContainsKey(0))
            {
                UserObjectId = parameters[0].ObjectToLong();
            }

            if (parameters.ContainsKey(2))
            {
                BuildingObjectId = parameters[2].ObjectToLong() ?? -1;
            }
        }
        catch (Exception e)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
        }
    }
}