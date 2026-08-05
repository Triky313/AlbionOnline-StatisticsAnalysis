using StatisticsAnalysisTool.Diagnostics;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StatisticsAnalysisTool.Network.Events;

public sealed class ItemRerollQualityFinishedEvent
{
    public IReadOnlyList<long> ResultItemObjectIds { get; } = [];
    public IReadOnlyList<long> SourceItemObjectIds { get; } = [];

    public ItemRerollQualityFinishedEvent(Dictionary<byte, object> parameters)
    {
        try
        {
            ResultItemObjectIds = parameters.TryGetValue(3, out var resultItems)
                ? NetworkParameterParser.GetLongValues(resultItems)
                : [];
            SourceItemObjectIds = parameters.TryGetValue(4, out var sourceItems)
                ? NetworkParameterParser.GetLongValues(sourceItems)
                : [];
        }
        catch (Exception exception)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, exception);
        }
    }
}