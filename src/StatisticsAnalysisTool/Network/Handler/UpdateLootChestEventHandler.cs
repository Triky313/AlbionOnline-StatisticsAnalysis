using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class UpdateLootChestEventHandler : EventPacketHandler<UpdateLootChestEvent>
{
    private readonly IGameEventWrapper _gameEventWrapper;

    public UpdateLootChestEventHandler(IGameEventWrapper gameEventWrapper) : base((int) EventCodes.UpdateLootChest)
    {
        _gameEventWrapper = gameEventWrapper;
    }

    protected override async Task OnActionAsync(UpdateLootChestEvent value)
    {
        _gameEventWrapper.DungeonController?.SetDungeonChestOpen(value.ObjectId, value.PlayerGuid);
        _gameEventWrapper?.TreasureController?.UpdateTreasure(value.ObjectId, value.PlayerGuid);
        await Task.CompletedTask;
    }
}