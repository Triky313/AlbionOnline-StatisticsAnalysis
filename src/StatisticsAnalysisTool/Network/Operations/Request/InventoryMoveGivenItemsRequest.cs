using Serilog;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Diagnostics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace StatisticsAnalysisTool.Network.Operations.Request;

public class InventoryMoveGivenItemsRequest
{
    public readonly Guid? ContainerGuid;
    public readonly Guid? UserInteractGuid;
    public readonly IReadOnlyList<long> ItemObjectIds;
    public readonly IReadOnlyList<int> Quantities;

    public InventoryMoveGivenItemsRequest(Dictionary<byte, object> parameters)
    {
        ItemObjectIds = [];
        Quantities = [];

        try
        {
            if (parameters.ContainsKey(0))
            {
                ContainerGuid = parameters[0].ObjectToGuid();
            }

            if (parameters.ContainsKey(2))
            {
                UserInteractGuid = parameters[2].ObjectToGuid();
            }

            if (parameters.ContainsKey(4))
            {
                ItemObjectIds = ObjectToLongList(parameters[4]);
            }

            if (parameters.ContainsKey(5))
            {
                Quantities = ObjectToIntList(parameters[5]);
            }
        }
        catch (Exception e)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
        }
    }

    private static IReadOnlyList<long> ObjectToLongList(object value)
    {
        var values = new List<long>();

        if (value is string || value is not IEnumerable enumerable)
        {
            return values;
        }

        foreach (var item in enumerable)
        {
            var longValue = item.ObjectToLong();
            if (longValue != null)
            {
                values.Add((long) longValue);
            }
        }

        return values;
    }

    private static IReadOnlyList<int> ObjectToIntList(object value)
    {
        var values = new List<int>();

        if (value is string || value is not IEnumerable enumerable)
        {
            return values;
        }

        foreach (var item in enumerable)
        {
            values.Add(item.ObjectToInt());
        }

        return values;
    }
}
