using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Trade.Market;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Serilog;

namespace StatisticsAnalysisTool.Network.Operations.Responses;

public class AuctionGetLoadoutOffersResponse
{
    public readonly List<AuctionEntry> AuctionEntries = new();
    public readonly List<int> NumberToBuyList = new();

    public AuctionGetLoadoutOffersResponse(Dictionary<byte, object> parameters)
    {
        ConsoleManager.WriteLine(new ConsoleFragment(GetType().Name, parameters, ConsoleColorType.EventColor));

        try
        {
            if (parameters.TryGetValue(1, out object auctionEntries))
            {
                if (auctionEntries is string[][] auctionEntriesList)
                {
                    var allRecords = auctionEntriesList.SelectMany(x => x).ToList();
                    foreach (var auctionOfferString in allRecords)
                    {
                        AuctionEntries.Add(JsonSerializer.Deserialize<AuctionEntry>(auctionOfferString ?? string.Empty));
                    }
                }
            }

            if (parameters.TryGetValue(2, out object buyQuantityNumbers))
            {
                if (buyQuantityNumbers is not short[][] quantity)
                {
                    return;
                }

                var allRecords = quantity.SelectMany(x => x).ToList();
                foreach (var auctionOfferString in allRecords)
                {
                    NumberToBuyList.Add(JsonSerializer.Deserialize<int>(auctionOfferString));
                }
            }
        }
        catch (Exception e)
        {
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
        }
    }
}