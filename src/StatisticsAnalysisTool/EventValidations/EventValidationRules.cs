using StatisticsAnalysisTool.Network;
using System;
using System.Collections.Generic;

namespace StatisticsAnalysisTool.EventValidations;

public static class EventValidationRules
{
    public static readonly Dictionary<EventCodes, Func<Dictionary<byte, object>, bool>> Rules = new()
        {
            { EventCodes.NewShrine, NewShrineEvent },
        };

    private static bool NewShrineEvent(Dictionary<byte, object> parameters)
    {
        return parameters.ContainsKey(0) 
               && parameters.ContainsKey(3)
               && parameters[3].ToString().Contains("SHRINE")
               && int.TryParse(parameters[0].ToString(), out var id) && id > 0;
    }
}