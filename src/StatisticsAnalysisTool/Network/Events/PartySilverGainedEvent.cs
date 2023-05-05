using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Network.Time;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace StatisticsAnalysisTool.Network.Events;

public class PartySilverGainedEvent
{
    public FixPoint SilverNet;
    public FixPoint SilverPreTax;
    public long? TargetEntityId;

    public GameTimeStamp TimeStamp;

    public PartySilverGainedEvent(Dictionary<byte, object> parameters)
    {
        ConsoleManager.WriteLineForNetworkHandler(GetType().Name, parameters);

        try
        {
            foreach (var parameter in parameters) Debug.Print($"{parameter}");

            if (parameters.ContainsKey(0)) TimeStamp = new GameTimeStamp(parameters[0].ObjectToLong() ?? 0);

            if (parameters.ContainsKey(1)) TargetEntityId = parameters[1].ObjectToLong();

            if (parameters.ContainsKey(2)) SilverNet = FixPoint.FromInternalValue(parameters[2].ObjectToLong() ?? 0);

            if (parameters.ContainsKey(3)) SilverPreTax = FixPoint.FromInternalValue(parameters[3].ObjectToLong() ?? 0);
        }
        catch (Exception e)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
        }
    }
}