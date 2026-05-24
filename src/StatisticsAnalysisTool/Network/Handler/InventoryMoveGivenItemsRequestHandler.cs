using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Network.Operations.Request;
using System;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class InventoryMoveGivenItemsRequestHandler(TrackingController trackingController) : RequestPacketHandler<InventoryMoveGivenItemsRequest>((int) OperationCodes.InventoryMoveGivenItems)
{
    protected override async Task OnActionAsync(InventoryMoveGivenItemsRequest value)
    {
        if (value.ContainerGuid is not null && value.UserInteractGuid is not null && value.ItemObjectIds.Count > 0)
        {
            await trackingController.DungeonController.AddNewLocalPlayerLootOnCurrentDungeonAsync(value.ItemObjectIds, (Guid) value.ContainerGuid, (Guid) value.UserInteractGuid);
            await trackingController.LootController.AddNewLocalPlayerLootAsync(value.ItemObjectIds, (Guid) value.ContainerGuid, (Guid) value.UserInteractGuid);
        }
    }
}