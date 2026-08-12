using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class UpdateLootChestEventHandler(TrackingController trackingController) : EventPacketHandler<UpdateLootChestEvent>((int) EventCodes.UpdateLootChest)
{
    protected override async Task OnActionAsync(UpdateLootChestEvent value)
    {
        trackingController.DungeonController?.SetDungeonChestOpen(value.ObjectId, value.PlayerGuid);
        trackingController.DungeonController?.UpdateCurrentDungeonLevelFromLootChest(value.ObjectId, value.LootFactor);
        trackingController?.TreasureController?.UpdateTreasure(value.ObjectId, value.PlayerGuid);
        await Task.CompletedTask;
    }
}