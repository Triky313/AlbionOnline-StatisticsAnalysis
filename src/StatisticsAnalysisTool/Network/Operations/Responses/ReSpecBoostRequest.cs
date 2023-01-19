using log4net;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Models;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StatisticsAnalysisTool.Network.Operations.Responses;

public class ReSpecBoostRequest
{
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

    public readonly bool IsReSpecBoostActive;

    public ReSpecBoostRequest(Dictionary<byte, object> parameters)
    {
        ConsoleManager.WriteLine(new ConsoleFragment(GetType().Name, parameters, ConsoleColorType.EventColor));

        try
        {
            if (parameters.ContainsKey(1))
            {
                IsReSpecBoostActive = parameters[1].ObjectToBool();
            }
        }
        catch (Exception e)
        {
            Log.Error(nameof(ReadMailResponse), e);
        }
    }
}