using Serilog;
using StatisticsAnalysisTool.Trade.Market;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;

namespace StatisticsAnalysisTool.Network.Operations.Responses;

public class AuctionGetRequests
{
    public readonly List<AuctionEntry> AuctionEntries = new();

    public AuctionGetRequests(Dictionary<byte, object> parameters)
    {
        try
        {
            if (parameters.ContainsKey(0))
            {
                foreach (var auctionOfferString in (IEnumerable<string>) parameters[0] ?? new List<string>())
                {
                    AuctionEntries.Add(JsonSerializer.Deserialize<AuctionEntry>(auctionOfferString ?? string.Empty));
                }
            }
        }
        catch (Exception e)
        {
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
        }
    }
}