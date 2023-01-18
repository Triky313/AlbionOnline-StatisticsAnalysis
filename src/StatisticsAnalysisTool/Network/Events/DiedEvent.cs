using StatisticsAnalysisTool.Common;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StatisticsAnalysisTool.Network.Events;

public class DiedEvent
{
    public DiedEvent(Dictionary<byte, object> parameters)
    {
        ConsoleManager.WriteLineForNetworkHandler(GetType().Name, parameters);

        try
        {
            if (parameters.ContainsKey(2))
            {
                Died = string.IsNullOrEmpty(parameters[2].ToString()) ? string.Empty : parameters[2].ToString();
            }

            if (parameters.ContainsKey(4))
            {
                KilledBy = string.IsNullOrEmpty(parameters[4].ToString()) ? string.Empty : parameters[4].ToString();
            }

            if (parameters.ContainsKey(5))
            {
                KilledByGuild = string.IsNullOrEmpty(parameters[5].ToString()) ? string.Empty : parameters[5].ToString();
            }
        }
        catch (Exception e)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
        }
    }

    public string Died { get; }

    public string KilledBy { get; }

    public string KilledByGuild { get; }
}