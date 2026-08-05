using Serilog;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StatisticsAnalysisTool.Network.Operations.Responses;

public class ActionOnBuildingStartRequest
{
    public readonly long Ticks;
    public readonly long BuildingObjectId;
    public readonly ActionOnBuildingType ActionType;
    public readonly long Costs;
    public readonly long AwakenedWeaponSilverCosts;
    public readonly int ItemIndex;
    public readonly int Quantity;
    public readonly IReadOnlyList<long> ItemObjectIds = [];
    public readonly IReadOnlyList<int> ItemQuantities = [];
    public readonly IReadOnlyList<ItemQuality> ItemQualities = [];

    public ActionOnBuildingStartRequest(Dictionary<byte, object> parameters)
    {
        try
        {
            if (parameters.TryGetValue(0, out object timestamp))
            {
                Ticks = timestamp.ObjectToLong() ?? 0;
            }

            if (parameters.TryGetValue(1, out object buildingObjectId))
            {
                BuildingObjectId = buildingObjectId.ObjectToLong() ?? -1;
            }

            if (parameters.TryGetValue(2, out object actionType))
            {
                var actionTypeNumber = actionType.ObjectToLong() ?? -1;
                ActionType = (ActionOnBuildingType) actionTypeNumber;
            }

            if (parameters.TryGetValue(4, out object costs))
            {
                Costs = costs.ObjectToLong() ?? 0;
            }

            if (parameters.TryGetValue(17, out object awakenedWeaponSilverCosts))
            {
                AwakenedWeaponSilverCosts = awakenedWeaponSilverCosts.ObjectToLong() ?? 0;
            }

            if (parameters.TryGetValue(5, out object itemObjectIds))
            {
                ItemObjectIds = NetworkParameterParser.GetLongValues(itemObjectIds);
            }

            if (parameters.TryGetValue(6, out object itemQuantities))
            {
                ItemQuantities = NetworkParameterParser.GetIntValues(itemQuantities);
            }

            if (parameters.TryGetValue(7, out object itemIndex))
            {
                ItemIndex = itemIndex.ObjectToInt();
            }

            if (parameters.TryGetValue(9, out object quantity))
            {
                Quantity = quantity.ObjectToInt();
            }

            if (parameters.TryGetValue(13, out object qualityLevel))
            {
                var itemQualities = new List<ItemQuality>();
                foreach (var qualityValue in NetworkParameterParser.GetIntValues(qualityLevel))
                {
                    itemQualities.Add(ItemController.GetQuality(qualityValue));
                }

                ItemQualities = itemQualities;
            }
        }
        catch (Exception e)
        {
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
        }
    }
}