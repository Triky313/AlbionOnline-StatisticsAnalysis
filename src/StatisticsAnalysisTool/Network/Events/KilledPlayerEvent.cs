using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Diagnostics;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StatisticsAnalysisTool.Network.Events;

public sealed class KilledPlayerEvent
{
    public KilledPlayerEvent(Dictionary<byte, object> parameters)
    {
        try
        {
            if (parameters.TryGetValue(0, out var killerObjectId))
            {
                KillerObjectId = killerObjectId.ObjectToLong() ?? 0;
            }

            if (parameters.TryGetValue(1, out var killedPlayerObjectId))
            {
                KilledPlayerObjectId = killedPlayerObjectId.ObjectToLong() ?? 0;
            }

            if (parameters.TryGetValue(2, out var killedPlayerName))
            {
                KilledPlayerName = killedPlayerName?.ToString() ?? string.Empty;
            }
        }
        catch (Exception exception)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, exception);
        }
    }

    public long KillerObjectId { get; }
    public long KilledPlayerObjectId { get; }
    public string KilledPlayerName { get; } = string.Empty;
}