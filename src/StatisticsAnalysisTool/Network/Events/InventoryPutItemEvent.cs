using StatisticsAnalysisTool.Common;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StatisticsAnalysisTool.Network.Events;

public class InventoryPutItemEvent
{
    public long? ObjectId { get; }
    public int InventorySlot { get; }
    public Guid? InteractGuid { get; }

    public InventoryPutItemEvent(Dictionary<byte, object> parameters)
    {
        ConsoleManager.WriteLineForNetworkHandler(GetType().Name, parameters);

        try
        {
            if (parameters.ContainsKey(0))
            {
                ObjectId = parameters[0].ObjectToLong();
            }

            InventorySlot = parameters.ContainsKey(1) ? parameters[1].ObjectToInt() : 0;

            if (parameters.ContainsKey(2))
            {
                InteractGuid = parameters[2].ObjectToGuid();
            }
        }
        catch (Exception e)
        {
            ObjectId = null;
            InteractGuid = null;
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
        }
    }
}