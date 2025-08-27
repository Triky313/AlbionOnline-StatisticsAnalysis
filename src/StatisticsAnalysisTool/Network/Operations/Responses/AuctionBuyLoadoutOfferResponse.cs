using Serilog;
using StatisticsAnalysisTool.Diagnostics;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StatisticsAnalysisTool.Network.Operations.Responses;

public class AuctionBuyLoadoutOfferResponse
{
    public List<long> PurchaseIds = new();

    public AuctionBuyLoadoutOfferResponse(Dictionary<byte, object> parameters)
    {
        try
        {
            if (parameters.TryGetValue(3, out object numberToBuyArray))
            {
                foreach (var numberToBuy in (IEnumerable<long>) numberToBuyArray ?? new List<long>())
                {
                    PurchaseIds.Add(numberToBuy);
                }
            }

        }
        catch (Exception e)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
        }
    }
}