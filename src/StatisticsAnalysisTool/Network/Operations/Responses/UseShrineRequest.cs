
using StatisticsAnalysisTool.Common;
using System;
using System.Collections.Generic;
using System.Reflection;
using Serilog;

namespace StatisticsAnalysisTool.Network.Operations.Responses;

public class UseShrineRequest
{
    public UseShrineRequest(Dictionary<byte, object> parameters)
    {
        ConsoleManager.WriteLineForNetworkHandler(GetType().Name, parameters);

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
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
        }
    }
}