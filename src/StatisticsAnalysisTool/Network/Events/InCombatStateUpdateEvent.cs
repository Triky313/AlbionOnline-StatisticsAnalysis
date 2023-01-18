using System;
using System.Collections.Generic;
using System.Reflection;
using log4net;
using StatisticsAnalysisTool.Common;

namespace StatisticsAnalysisTool.Network.Events;

public class InCombatStateUpdateEvent : BaseEvent
{
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);
    public readonly bool InActiveCombat;
    public readonly bool InPassiveCombat;

    public long? ObjectId;

    public InCombatStateUpdateEvent(Dictionary<byte, object> parameters) : base(parameters)
    {
        ConsoleManager.WriteLineForNetworkHandler(GetType().Name, parameters);

        try
        {
            if (parameters.ContainsKey(0))
            {
                ObjectId = parameters[0].ObjectToLong();
            }

            if (parameters.ContainsKey(1))
            {
                InActiveCombat = parameters[1] as bool? ?? false;
            }

            if (parameters.ContainsKey(2))
            {
                InPassiveCombat = parameters[2] as bool? ?? false;
            }
        }
        catch (Exception e)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, e);
        }
    }
}