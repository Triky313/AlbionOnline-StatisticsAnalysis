using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Network.Operations.Responses;
using System;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class InventoryMoveItemRequestHandler : RequestPacketHandler<InventoryMoveItemRequest>
{
    private readonly IGameEventWrapper _gameEventWrapper;

    public InventoryMoveItemRequestHandler(IGameEventWrapper gameEventWrapper) : base((int) OperationCodes.InventoryMoveItem)
    {
        _gameEventWrapper = gameEventWrapper;
    }

    protected override async Task OnActionAsync(InventoryMoveItemRequest value)
    {
        if (value.ContainerGuid is not null && value.UserInteractGuid is not null)
        {
            await _gameEventWrapper.DungeonController.AddNewLocalPlayerLootOnCurrentDungeonAsync(value.ContainerSlot, (Guid) value.ContainerGuid, (Guid) value.UserInteractGuid);
            await _gameEventWrapper.LootController.AddNewLocalPlayerLootAsync(value.ContainerSlot, (Guid) value.ContainerGuid, (Guid) value.UserInteractGuid);
        }
    }
}