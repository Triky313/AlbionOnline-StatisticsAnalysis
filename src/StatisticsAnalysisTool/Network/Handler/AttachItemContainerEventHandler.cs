using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class AttachItemContainerEventHandler : EventPacketHandler<AttachItemContainerEvent>
{
    private readonly IGameEventWrapper _gameEventWrapper;

    public AttachItemContainerEventHandler(IGameEventWrapper gameEventWrapper) : base((int) EventCodes.AttachItemContainer)
    {
        _gameEventWrapper = gameEventWrapper;
    }

    protected override async Task OnActionAsync(AttachItemContainerEvent value)
    {
        if (_gameEventWrapper.TrackingController.IsTrackingAllowedByMainCharacter())
        {
            _gameEventWrapper.VaultController.AddContainer(value.ItemContainerObject);
        }

        _gameEventWrapper.LootController.SetCurrentItemContainer(value.ItemContainerObject);
        _gameEventWrapper.DungeonController.SetCurrentItemContainer(value.ItemContainerObject);
        await Task.CompletedTask;
    }
}