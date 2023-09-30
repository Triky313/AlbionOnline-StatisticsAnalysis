using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Models.NetworkModel;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StatisticsAnalysisTool.Network.Events;

public class NewKillTrophyItemEvent
{
    public readonly DiscoveredItem Item;

    private readonly long? _objectId;
    private readonly int _itemId;
    private readonly int _quantity;
    private readonly long _estimatedMarketValue;
    private readonly FixPoint _durability;

    public NewKillTrophyItemEvent(Dictionary<byte, object> parameters)
    {
        ConsoleManager.WriteLineForNetworkHandler(GetType().Name, parameters);

        try
        {
            if (parameters.TryGetValue(0, out object objectId))
            {
                _objectId = objectId.ObjectToLong();
            }

            if (parameters.TryGetValue(1, out object itemId))
            {
                _itemId = itemId.ObjectToInt();
            }

            if (parameters.TryGetValue(2, out object quantity))
            {
                _quantity = quantity.ObjectToInt();
            }

            if (parameters.TryGetValue(4, out object estimatedMarketValue))
            {
                _estimatedMarketValue = estimatedMarketValue.ObjectToLong() ?? 0;
            }

            if (parameters.TryGetValue(7, out object durability))
            {
                _durability = FixPoint.FromInternalValue(durability.ObjectToLong() ?? 0);
            }

            if (_objectId != null)
            {
                Item = new DiscoveredItem()
                {
                    ObjectId = (long) _objectId,
                    ItemIndex = _itemId,
                    Quantity = _quantity,
                    CurrentDurability = _durability,
                    EstimatedMarketValueInternal = _estimatedMarketValue
                };
            }
            else
            {
                Item = null;
            }
        }
        catch (Exception e)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
        }
    }
}