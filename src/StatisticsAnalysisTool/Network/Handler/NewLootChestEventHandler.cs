using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;
using StatisticsAnalysisTool.Network.Events;

namespace StatisticsAnalysisTool.Network.Handler;

public class NewLootChestEventHandler : EventPacketHandler<NewLootChestEvent>
{
    private readonly IGameEventWrapper _gameEventWrapper;

    public NewLootChestEventHandler(IGameEventWrapper gameEventWrapper) : base((int) EventCodes.NewLootChest)
    {
        _gameEventWrapper = gameEventWrapper;
    }

    protected override async Task OnActionAsync(NewLootChestEvent value)
    {
        await _gameEventWrapper?.DungeonController?.SetDungeonEventInformationAsync(value.ObjectId, value.UniqueName)!;
        _gameEventWrapper?.TreasureController?.AddTreasure(value.ObjectId, value.UniqueName, value.UniqueNameWithLocation);
    }
}