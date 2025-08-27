using Serilog;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Trade.Market;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StatisticsAnalysisTool.Network.Operations.Responses;

public class AuctionSellSpecificItemRequest
{
    public readonly Sale Sale;

    public AuctionSellSpecificItemRequest(Dictionary<byte, object> parameters)
    {
        try
        {
            long objectId = -1;
            int amount = -1;
            int itemId = -1;
            long auctionId = -1;

            if (parameters.ContainsKey(0))
            {
                objectId = parameters[0].ObjectToLong() ?? -1;
            }

            if (parameters.ContainsKey(1))
            {
                auctionId = parameters[1].ObjectToLong() ?? -1;
            }

            if (parameters.ContainsKey(2))
            {
                itemId = parameters[2].ObjectToInt();
            }

            if (parameters.ContainsKey(4))
            {
                amount = parameters[4].ObjectToInt();
            }


            Sale = new Sale()
            {
                ObjectId = objectId,
                AuctionId = auctionId,
                ItemId = itemId,
                Amount = amount
            };
        }
        catch (Exception e)
        {
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
        }
    }
}