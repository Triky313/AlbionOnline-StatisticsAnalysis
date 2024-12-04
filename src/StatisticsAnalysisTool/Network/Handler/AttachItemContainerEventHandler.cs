using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class AttachItemContainerEventHandler : EventPacketHandler<AttachItemContainerEvent>
{
    private readonly TrackingController _trackingController;

    public AttachItemContainerEventHandler(TrackingController trackingController) : base((int) EventCodes.AttachItemContainer)
    {
        _trackingController = trackingController;
    }

    protected override async Task OnActionAsync(AttachItemContainerEvent value)
    {
        if (_trackingController.IsTrackingAllowedByMainCharacter())
        {
            _trackingController.VaultController.AddContainer(value.ItemContainerObject);
        }
        
        _trackingController.LootController.SetCurrentItemContainer(value.ItemContainerObject);
        _trackingController.DungeonController.SetCurrentItemContainer(value.ItemContainerObject);
        await Task.CompletedTask;
    }
}