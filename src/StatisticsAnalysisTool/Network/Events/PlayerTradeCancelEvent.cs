using Serilog;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Diagnostics;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StatisticsAnalysisTool.Network.Events;

public sealed class PlayerTradeCancelEvent
{
    public long TradeId { get; }

    public PlayerTradeCancelEvent(Dictionary<byte, object> parameters)
    {
        try
        {
            TradeId = parameters.TryGetValue(0, out var tradeId) ? tradeId.ObjectToLong() ?? 0 : 0;
        }
        catch (Exception e)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
        }
    }
}