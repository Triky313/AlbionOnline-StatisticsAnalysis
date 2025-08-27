using System;
using System.Collections.Generic;
using System.Reflection;
using StatisticsAnalysisTool.Diagnostics;

namespace StatisticsAnalysisTool.Network.Events;

public class TreasureChestUsingStartEvent
{
    public string Username;

    public TreasureChestUsingStartEvent(Dictionary<byte, object> parameters)
    {
        try
        {
            if (parameters.ContainsKey(2))
            {
                Username = string.IsNullOrEmpty(parameters[2].ToString()) ? string.Empty : parameters[2].ToString();
            }
        }
        catch (Exception e)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
        }
    }
}