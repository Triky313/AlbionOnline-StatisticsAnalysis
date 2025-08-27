using Serilog;
using StatisticsAnalysisTool.Common;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StatisticsAnalysisTool.Network.Operations.Responses;

public class UnRegisterFromObjectRequest
{
    public readonly long BuildingObjectId;

    public UnRegisterFromObjectRequest(Dictionary<byte, object> parameters)
    {
        try
        {
            if (parameters.ContainsKey(0))
            {
                BuildingObjectId = parameters[0].ObjectToLong() ?? -1;
            }
        }
        catch (Exception e)
        {
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
        }
    }
}