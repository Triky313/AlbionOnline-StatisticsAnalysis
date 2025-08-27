using StatisticsAnalysisTool.Common;
using System;
using System.Collections.Generic;
using System.Reflection;
using StatisticsAnalysisTool.Diagnostics;

namespace StatisticsAnalysisTool.Network.Events;

public class InventoryDeleteItemEvent
{
    public long? ObjectId { get; }

    public InventoryDeleteItemEvent(Dictionary<byte, object> parameters)
    {
        try
        {
            if (parameters.ContainsKey(0))
            {
                ObjectId = parameters[0].ObjectToLong();
            }
        }
        catch (Exception e)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
        }
    }
}