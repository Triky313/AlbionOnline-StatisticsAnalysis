using System;
using System.Collections.Generic;
using System.Reflection;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;

namespace StatisticsAnalysisTool.Network.Events;

public class ActionOnBuildingFinishedEvent : BaseEvent
{
    public long? UserObjectId;
    public readonly long BuildingObjectId;
    public readonly ActionOnBuildingType ActionType;

    public ActionOnBuildingFinishedEvent(Dictionary<byte, object> parameters) : base(parameters)
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

            if (parameters.ContainsKey(4))
            {
                var actionTypeNumber = parameters[4].ObjectToLong() ?? 0;
                ActionType = (ActionOnBuildingType)actionTypeNumber;
            }
        }
        catch (Exception e)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
        }
    }
}