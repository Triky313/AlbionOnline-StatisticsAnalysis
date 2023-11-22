using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Properties;
using System;
using System.Collections.Generic;
using System.Reflection;
using StatisticsAnalysisTool.Enumerations;

namespace StatisticsAnalysisTool.Network.Events;

public class NewExpeditionCheckPointEvent
{
    public int ObjectId { get; set; }
    public CheckPointStatus Status { get; set; }

    public NewExpeditionCheckPointEvent(Dictionary<byte, object> parameters)
    {
        if (Settings.Default.IsDebugMode)
        {
            ConsoleManager.WriteLineForDebug(GetType().Name, parameters);
        }
        else
        {
            ConsoleManager.WriteLineForNetworkHandler(GetType().Name, parameters);
        }

        try
        {
            if (parameters.ContainsKey(0) && int.TryParse(parameters[0].ToString(), out var objectId))
            {
                ObjectId = objectId;
            }

            if (parameters.TryGetValue(2, out object checkPointStatus))
            {
                Status = GetCheckPointStatus(checkPointStatus.ObjectToShort());
            }
        }
        catch (Exception e)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
        }
    }

    private static CheckPointStatus GetCheckPointStatus(int? number)
    {
        if (number is null)
        {
            return CheckPointStatus.Unknown;
        }

        if (number == 1)
        {
            return CheckPointStatus.Active;
        }

        if (number == 2)
        {
            return CheckPointStatus.Done;
        }

        return CheckPointStatus.Unknown;
    }
}