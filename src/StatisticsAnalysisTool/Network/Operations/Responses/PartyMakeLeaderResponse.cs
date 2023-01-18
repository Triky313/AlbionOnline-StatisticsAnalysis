using log4net;
using StatisticsAnalysisTool.Common;
using System;
using System.Collections.Generic;
using System.Reflection;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Models;

namespace StatisticsAnalysisTool.Network.Operations.Responses;

public class PartyMakeLeaderResponse
{
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

    public string Username;

    public PartyMakeLeaderResponse(Dictionary<byte, object> parameters)
    {
        ConsoleManager.WriteLine(new ConsoleFragment(GetType().Name, parameters, ConsoleColorType.EventColor));

        try
        {
            if (parameters.ContainsKey(0))
            {
                Username = string.IsNullOrEmpty(parameters[0].ToString()) ? string.Empty : parameters[0].ToString();
            }
        }
        catch (Exception e)
        {
            Log.Error(nameof(PartyMakeLeaderResponse), e);
        }
    }
}