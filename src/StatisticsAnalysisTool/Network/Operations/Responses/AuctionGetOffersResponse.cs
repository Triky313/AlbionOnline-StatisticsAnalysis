using Serilog;
using StatisticsAnalysisTool.Trade.Market;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;

namespace StatisticsAnalysisTool.Network.Operations.Responses;

public class AuctionGetOffersResponse
{
    public readonly List<AuctionEntry> AuctionEntries = [];

    public AuctionGetOffersResponse(Dictionary<byte, object> parameters)
    {
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