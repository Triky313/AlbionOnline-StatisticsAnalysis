
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Models;
using System;
using System.Collections.Generic;
using System.Reflection;
using Serilog;

namespace StatisticsAnalysisTool.Network.Operations.Responses;

public class AuctionBuyLoadoutOfferResponse
{
    

    public List<long> PurchaseIds = new();

    public AuctionBuyLoadoutOfferResponse(Dictionary<byte, object> parameters)
    {
        ConsoleManager.WriteLine(new ConsoleFragment(GetType().Name, parameters, ConsoleColorType.EventColor));

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
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
        }
    }
}