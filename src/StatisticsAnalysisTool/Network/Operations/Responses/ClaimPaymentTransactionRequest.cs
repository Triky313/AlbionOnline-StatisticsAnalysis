using Serilog;
using StatisticsAnalysisTool.Common;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StatisticsAnalysisTool.Network.Operations.Responses;

public class ClaimPaymentTransactionRequest
{
    public readonly bool IsReSpecBoostActive;

    public ClaimPaymentTransactionRequest(Dictionary<byte, object> parameters)
    {
        try
        {
            if (parameters.ContainsKey(1))
            {
                IsReSpecBoostActive = parameters[1].ObjectToBool();
            }
        }
        catch (Exception e)
        {
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
        }
    }
}