using Serilog;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Diagnostics;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StatisticsAnalysisTool.Network.Events;

public sealed class InvitationPlayerTradeEvent
{
    public long PartnerObjectId { get; }
    public string PartnerName { get; } = string.Empty;
    public string PartnerGuildName { get; } = string.Empty;
    public long TradeId { get; }

    public InvitationPlayerTradeEvent(Dictionary<byte, object> parameters)
    {
        try
        {
            PartnerObjectId = GetLong(parameters, 0);
            PartnerName = GetString(parameters, 1);
            PartnerGuildName = GetString(parameters, 2);
            TradeId = GetLong(parameters, 6);
        }
        catch (Exception e)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
        }
    }

    private static long GetLong(IReadOnlyDictionary<byte, object> parameters, byte key)
    {
        return parameters.TryGetValue(key, out var value) ? value.ObjectToLong() ?? 0 : 0;
    }

    private static string GetString(IReadOnlyDictionary<byte, object> parameters, byte key)
    {
        return parameters.TryGetValue(key, out var value) ? value?.ToString() ?? string.Empty : string.Empty;
    }
}