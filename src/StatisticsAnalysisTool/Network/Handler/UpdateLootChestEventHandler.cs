using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class UpdateLootChestEventHandler(TrackingController trackingController) : EventPacketHandler<UpdateLootChestEvent>((int) EventCodes.UpdateLootChest)
{
    protected override async Task OnActionAsync(UpdateLootChestEvent value)
    {
        var trackedRarity = await trackingController.DungeonController.UpdateDungeonChestAsync(
            value.ObjectId,
            value.PlayerGuid,
            value.IsOpened,
            value.Rarity);
        trackingController.DungeonController?.UpdateCurrentDungeonLevelFromLootChest(value.ObjectId, value.LootFactor);
        trackingController?.TreasureController?.UpdateTreasure(
            value.ObjectId,
            value.PlayerGuid,
            value.IsOpened,
            value.Rarity != TreasureRarity.Unknown ? value.Rarity : trackedRarity);
    }
}