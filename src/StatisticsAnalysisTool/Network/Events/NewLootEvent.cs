using StatisticsAnalysisTool.Common;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StatisticsAnalysisTool.Network.Events;

public class NewLootEvent : BaseEvent
{
    public readonly long? ObjectId;
    public readonly string LootBody;

    public NewLootEvent(Dictionary<byte, object> parameters) :base(parameters)
    {
        ConsoleManager.WriteLineForNetworkHandler(GetType().Name, parameters);

        try
        {
            if (parameters.ContainsKey(0))
            {
                ObjectId = parameters[0].ObjectToLong();
            }

            if (parameters.ContainsKey(3))
            {
                LootBody = string.IsNullOrEmpty(parameters[3].ToString()) ? string.Empty : parameters[3].ToString();
            }
        }
        catch (Exception e)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
        }
    }
}