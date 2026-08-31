using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Diagnostics;
using StatisticsAnalysisTool.Models.NetworkModel;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StatisticsAnalysisTool.Network.Events;

public class NewSiegeBannerItemEvent
{
    public readonly DiscoveredItem Item;

    public NewSiegeBannerItemEvent(Dictionary<byte, object> parameters)
    {
        try
        {
            var objectId = parameters.TryGetValue(0, out var objectIdValue)
                ? objectIdValue.ObjectToLong()
                : null;

            if (objectId == null)
            {
                return;
            }

            Item = new DiscoveredItem
            {
                ObjectId = objectId.Value,
                ItemIndex = parameters.TryGetValue(1, out var itemIdValue) ? itemIdValue.ObjectToInt() : 0,
                Quantity = parameters.TryGetValue(2, out var quantityValue) ? quantityValue.ObjectToInt() : 0,
                EstimatedMarketValueInternal = parameters.TryGetValue(4, out var estimatedMarketValue)
                    ? estimatedMarketValue.ObjectToLong() ?? 0
                    : 0
            };
        }
        catch (Exception e)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
        }
    }
}