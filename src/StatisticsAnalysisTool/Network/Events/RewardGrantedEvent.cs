using StatisticsAnalysisTool.Common;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StatisticsAnalysisTool.Network.Events;

public class RewardGrantedEvent
{
    public RewardGrantedEvent(Dictionary<byte, object> parameters)
    {
        ConsoleManager.WriteLineForNetworkHandler(GetType().Name, parameters);

        try
        {
            if (parameters.TryGetValue(1, out object objectId))
            {
                ItemIndex = objectId.ObjectToInt();
            }

            if (parameters.TryGetValue(3, out object quantity))
            {
                Quantity = quantity.ObjectToInt();
            }
        }
        catch (Exception e)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
        }
    }

    public int ItemIndex { get; }
    public int Quantity { get; }
}