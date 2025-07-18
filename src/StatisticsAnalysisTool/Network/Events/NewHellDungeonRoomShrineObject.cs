using Serilog;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.EventValidations;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StatisticsAnalysisTool.Network.Events;

public class NewHellDungeonRoomShrineObjectEvent
{
    public NewHellDungeonRoomShrineObjectEvent(Dictionary<byte, object> parameters)
    {
        EventValidator.IsEventValid(EventCodes.NewShrine, parameters);
        ConsoleManager.WriteLineForNetworkHandler(GetType().Name, parameters);

        try
        {
            if (parameters.ContainsKey(0) && int.TryParse(parameters[0].ToString(), out var id))
            {
                Id = id;
            }

            if (parameters.ContainsKey(4))
            {
                ObjectName = string.IsNullOrEmpty(parameters[4].ToString()) ? string.Empty : parameters[4].ToString();
            }
        }
        catch (Exception e)
        {
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
        }
    }

    public int Id { get; set; }
    public string ObjectName { get; set; }
}