
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Trade.Market;
using System;
using System.Collections.Generic;
using System.Reflection;
using Serilog;

namespace StatisticsAnalysisTool.Network.Operations.Responses;

public class AuctionBuyOfferRequest
{
    

    public readonly Purchase Purchase;

    public AuctionBuyOfferRequest(Dictionary<byte, object> parameters)
    {
        ConsoleManager.WriteLine(new ConsoleFragment(GetType().Name, parameters, ConsoleColorType.EventColor));

        try
        {
            long objectId = -1;
            int amount = -1;
            long auctionId = -1;

            if (parameters.ContainsKey(0))
            {
                objectId = parameters[0].ObjectToLong() ?? -1;
            }

            if (parameters.ContainsKey(1))
            {
                amount = parameters[1].ObjectToInt();
            }

            if (parameters.ContainsKey(2))
            {
                auctionId = parameters[2].ObjectToLong() ?? -1;
            }

            Purchase = new Purchase()
            {
                ObjectId = objectId,
                Amount = amount,
                AuctionId = auctionId
            };
        }
        catch (Exception e)
        {
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
        }
    }
}