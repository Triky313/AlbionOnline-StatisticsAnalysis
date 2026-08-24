using StatisticsAnalysisTool.Enumerations;
﻿using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class NewLootChestEventHandler(TrackingController trackingController) : EventPacketHandler<NewLootChestEvent>((int) EventCodes.NewLootChest)
{
    protected override async Task OnActionAsync(NewLootChestEvent value)
    {
        var dungeonEventName = GetDungeonEventName(value);
        await trackingController.DungeonController.RegisterDungeonChestAsync(value.ObjectId, dungeonEventName, value.Rarity);
        trackingController?.TreasureController?.AddTreasure(value.ObjectId, value.UniqueName, value.UniqueNameWithLocation, value.Rarity);
        trackingController.LootController.SetIdentifiedBody(value.ObjectId, value.UniqueName);
        trackingController.DungeonController.SetLootSource(value.ObjectId, value.UniqueNameWithLocation, DungeonLootSourceType.Chest);
    }

    private static string GetDungeonEventName(NewLootChestEvent value)
    {
        return !string.IsNullOrWhiteSpace(value.UniqueName)
               && value.UniqueName.StartsWith("HD_DEMON_", System.StringComparison.Ordinal)
               && !string.IsNullOrWhiteSpace(value.UniqueNameWithLocation)
            ? value.UniqueNameWithLocation
            : value.UniqueName;
    }
}
