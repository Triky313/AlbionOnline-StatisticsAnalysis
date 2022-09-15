using StatisticsAnalysisTool.Common;
using System.Collections.Generic;
using System.Reflection;
using System;

namespace StatisticsAnalysisTool.Network.Events;

public class SiegeCampClaimStartEvent
{
    public string Username;

    public SiegeCampClaimStartEvent(Dictionary<byte, object> parameters)
    {
        ConsoleManager.WriteLineForNetworkHandler(GetType().Name, parameters);

        try
        {
            if (parameters.ContainsKey(2))
            {
                Username = string.IsNullOrEmpty(parameters[2].ToString()) ? string.Empty : parameters[2].ToString();
            }
        }
        catch (Exception e)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
        }
    }
}