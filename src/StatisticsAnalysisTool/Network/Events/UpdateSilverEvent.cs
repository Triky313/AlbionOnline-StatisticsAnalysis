using System;
using System.Collections.Generic;
using System.Reflection;
using log4net;
using StatisticsAnalysisTool.Common;

namespace StatisticsAnalysisTool.Network.Events;

public class UpdateSilverEvent : BaseEvent
{
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

    public UpdateSilverEvent(Dictionary<byte, object> parameters) : base(parameters)
    {
        ConsoleManager.WriteLineForNetworkHandler(GetType().Name, parameters);

        try
        {
            if (parameters.ContainsKey(1))
            {
                CurrentPlayerSilver = FixPoint.FromInternalValue(parameters[1].ObjectToLong() ?? 0);
            }
        }
        catch (ArgumentNullException e)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, e);
        }
    }

    public FixPoint CurrentPlayerSilver { get; }
}