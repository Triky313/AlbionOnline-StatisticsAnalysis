using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Trade.Market;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using Serilog;

namespace StatisticsAnalysisTool.Network.Operations.Responses;

public class AuctionGetResponse
{
    public readonly List<AuctionEntry> AuctionEntries = new();

    public AuctionGetResponse(Dictionary<byte, object> parameters)
    {
        ConsoleManager.WriteLine(new ConsoleFragment(GetType().Name, parameters, ConsoleColorType.EventColor));

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