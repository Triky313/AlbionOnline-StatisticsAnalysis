using Serilog;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Diagnostics;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StatisticsAnalysisTool.Network.Operations.Responses;

public sealed class InviteToPlayerTradeResponse
{
    public long PartnerObjectId { get; }
    public string PartnerName { get; } = string.Empty;
    public long TradeId { get; }

    public InviteToPlayerTradeResponse(Dictionary<byte, object> parameters)
    {
        try
        {
            PartnerObjectId = parameters.TryGetValue(0, out var partnerObjectId) ? partnerObjectId.ObjectToLong() ?? 0 : 0;
            PartnerName = parameters.TryGetValue(1, out var partnerName) ? partnerName?.ToString() ?? string.Empty : string.Empty;
            TradeId = parameters.TryGetValue(6, out var tradeId) ? tradeId.ObjectToLong() ?? 0 : 0;
        }
        catch (Exception e)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
        }
    }
}