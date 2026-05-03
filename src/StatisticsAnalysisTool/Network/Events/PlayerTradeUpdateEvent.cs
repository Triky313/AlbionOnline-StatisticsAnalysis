using Serilog;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Diagnostics;
using StatisticsAnalysisTool.Trade.PlayerTrades;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace StatisticsAnalysisTool.Network.Events;

public sealed class PlayerTradeUpdateEvent
{
    public PlayerTradeUpdate Update { get; }

    public PlayerTradeUpdateEvent(Dictionary<byte, object> parameters)
    {
        try
        {
            Update = new PlayerTradeUpdate
            {
                TradeId = GetLong(parameters, 0),
                Revision = GetLong(parameters, 1),
                LocalSilverInternal = GetLong(parameters, 2),
                PartnerSilverInternal = GetLong(parameters, 4),
                LocalItems = BuildItems(parameters, 8, 15),
                PartnerItems = BuildItems(parameters, 18, 25)
            };
        }
        catch (Exception e)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
            Update = new PlayerTradeUpdate();
        }
    }

    private static long GetLong(IReadOnlyDictionary<byte, object> parameters, byte key)
    {
        return parameters.TryGetValue(key, out var value) ? value.ObjectToLong() ?? 0 : 0;
    }

    private static IReadOnlyList<PlayerTradeItem> BuildItems(IReadOnlyDictionary<byte, object> parameters, byte itemIndexKey, byte quantityKey)
    {
        var itemIndexes = GetIntValues(parameters, itemIndexKey);
        var quantities = GetIntValues(parameters, quantityKey);
        var count = Math.Max(itemIndexes.Count, quantities.Count);
        var items = new List<PlayerTradeItem>(count);

        for (var i = 0; i < count; i++)
        {
            var itemIndex = GetValueOrDefault(itemIndexes, i);
            if (itemIndex <= 0)
            {
                continue;
            }

            var quantity = GetValueOrDefault(quantities, i);
            if (quantity <= 0)
            {
                quantity = 1;
            }

            items.Add(new PlayerTradeItem
            {
                ItemIndex = itemIndex,
                Quantity = quantity
            });
        }

        return items;
    }

    private static IReadOnlyList<int> GetIntValues(IReadOnlyDictionary<byte, object> parameters, byte key)
    {
        if (!parameters.TryGetValue(key, out var value) || value is not IEnumerable enumerable || value is string)
        {
            return [];
        }

        return enumerable
            .Cast<object>()
            .Select(x => x.ObjectToInt())
            .ToList();
    }

    private static int GetValueOrDefault(IReadOnlyList<int> values, int index)
    {
        return index >= 0 && index < values.Count ? values[index] : 0;
    }
}