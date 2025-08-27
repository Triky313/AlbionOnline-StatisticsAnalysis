using Serilog;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StatisticsAnalysisTool.Network.Operations.Responses;

public class PartyMakeLeaderResponse
{
    public string Username;

    public PartyMakeLeaderResponse(Dictionary<byte, object> parameters)
    {
        try
        {
            if (parameters.ContainsKey(0))
            {
                Username = string.IsNullOrEmpty(parameters[0].ToString()) ? string.Empty : parameters[0].ToString();
            }
        }
        catch (Exception e)
        {
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
        }
    }
}