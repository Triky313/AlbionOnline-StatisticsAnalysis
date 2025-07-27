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

            if (parameters.ContainsKey(3))
            {
                DiedPlayerGuild = string.IsNullOrEmpty(parameters[3].ToString()) ? string.Empty : parameters[3].ToString();
            }

            if (parameters.ContainsKey(10))
            {
                KilledBy = string.IsNullOrEmpty(parameters[10].ToString()) ? string.Empty : parameters[10].ToString();
            }

            if (parameters.ContainsKey(11))
            {
                KilledByGuild = string.IsNullOrEmpty(parameters[11].ToString()) ? string.Empty : parameters[11].ToString();
            }

        }
        catch (Exception e)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
        }
    }

    public string Died { get; }

    public string DiedPlayerGuild { get; }

    public string KilledBy { get; }

    public string KilledByGuild { get; }
}