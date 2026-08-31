using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Diagnostics;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StatisticsAnalysisTool.Network.Events;

public sealed class KnockedDownEvent
{
    private long _playerObjectId;

    public KnockedDownEvent(Dictionary<byte, object> parameters)
    {
        try
        {
            if (parameters.TryGetValue(0, out var knockedDownObjectId))
            {
                KnockedDownObjectId = knockedDownObjectId.ObjectToLong() ?? 0;
            }

            if (parameters.TryGetValue(2, out var knockedDownByObjectId))
            {
                KnockedDownByObjectId = knockedDownByObjectId.ObjectToLong() ?? 0;
            }

            if (parameters.TryGetValue(3, out var knockedDownByName))
            {
                KnockedDownByName = knockedDownByName?.ToString() ?? string.Empty;
            }

            if (parameters.TryGetValue(4, out var playerObjectId))
            {
                _playerObjectId = playerObjectId.ObjectToLong() ?? 0;
            }

            if (parameters.TryGetValue(5, out var playerName))
            {
                PlayerName = playerName?.ToString() ?? string.Empty;
            }
        }
        catch (Exception exception)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, exception);
        }
    }

    public long PlayerObjectId => _playerObjectId > 0 ? _playerObjectId : KnockedDownObjectId;
    public string PlayerName { get; } = string.Empty;
    public long KnockedDownByObjectId { get; }
    public string KnockedDownByName { get; } = string.Empty;

    private long KnockedDownObjectId { get; }
}