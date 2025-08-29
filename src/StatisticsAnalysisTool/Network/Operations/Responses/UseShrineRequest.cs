using Serilog;
using StatisticsAnalysisTool.Diagnostics;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StatisticsAnalysisTool.Network.Operations.Responses;

public class UseShrineRequest
{
    public UseShrineRequest(Dictionary<byte, object> parameters)
    {
        try
        {
            //Debug.Print("----- UseShrine (Operation) -----");

            //foreach (var parameter in parameters)
            //{
            //    Debug.Print($"{parameter}");
            //}
        }
        catch (Exception e)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
        }
    }
}