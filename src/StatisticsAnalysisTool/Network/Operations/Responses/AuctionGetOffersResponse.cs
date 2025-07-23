
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

public class AuctionGetOffersResponse
{
    public readonly List<AuctionEntry> AuctionEntries = [];

    public AuctionGetOffersResponse(Dictionary<byte, object> parameters)
    {
        ConsoleManager.WriteLine(new ConsoleFragment(GetType().Name, parameters, ConsoleColorType.EventColor));

        try
        {
            if (parameters.TryGetValue(0, out object auctionOffers))
            {
                foreach (var auctionOfferString in (IEnumerable<string>) auctionOffers ?? new List<string>())
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