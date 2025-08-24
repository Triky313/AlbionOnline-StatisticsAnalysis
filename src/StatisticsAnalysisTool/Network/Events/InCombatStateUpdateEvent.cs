using Serilog;
using StatisticsAnalysisTool.Common;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StatisticsAnalysisTool.Network.Events;

public class InCombatStateUpdateEvent
{
    public readonly bool InActiveCombat;
    public readonly bool InPassiveCombat;

    public long? ObjectId;

    public InCombatStateUpdateEvent(Dictionary<byte, object> parameters)
    {
        ConsoleManager.WriteLineForNetworkHandler(GetType().Name, parameters);

        try
        {
            if (parameters.ContainsKey(0))
            {
                ObjectId = parameters[0].ObjectToLong();
            }

            if (parameters.TryGetValue(1, out object inActiveCombat))
            {
                InActiveCombat = inActiveCombat as bool? ?? false;
            }

            if (parameters.TryGetValue(2, out object inPassiveCombat))
            {
                InPassiveCombat = inPassiveCombat as bool? ?? false;
            }
        }
        catch (Exception e)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
        }
    }
}