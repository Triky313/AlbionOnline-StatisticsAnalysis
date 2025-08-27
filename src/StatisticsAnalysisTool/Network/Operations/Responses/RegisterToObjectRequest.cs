using Serilog;
using StatisticsAnalysisTool.Common;
using System;
using System.Collections.Generic;

namespace StatisticsAnalysisTool.Network.Operations.Responses;

public class RegisterToObjectRequest
{


    public readonly long BuildingObjectId;

    public RegisterToObjectRequest(Dictionary<byte, object> parameters)
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
            Log.Error(nameof(ReadMailResponse), e);
        }
    }
}