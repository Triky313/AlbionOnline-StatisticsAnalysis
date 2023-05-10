using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Network.Operations.Responses;
using System;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class InventoryMoveItemRequestHandler : RequestPacketHandler<InventoryMoveItemRequest>
{
    private readonly TrackingController _trackingController;

    public InventoryMoveItemRequestHandler(TrackingController trackingController) : base((int) OperationCodes.InventoryMoveItem)
    {
        _trackingController = trackingController;
    }

    protected override async Task OnActionAsync(InventoryMoveItemRequest value)
    {
        if (value.ContainerGuid is not null && value.UserInteractGuid is not null)
        {
            _trackingController.DungeonController.AddNewLocalPlayerLootOnCurrentDungeon(value.ContainerSlot, (Guid)value.ContainerGuid, (Guid)value.UserInteractGuid);
            await _trackingController.LootController.AddNewLocalPlayerLootAsync(value.ContainerSlot, (Guid)value.ContainerGuid, (Guid)value.UserInteractGuid);
        }
    }
}