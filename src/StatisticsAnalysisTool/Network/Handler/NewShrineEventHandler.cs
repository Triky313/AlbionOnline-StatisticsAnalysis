using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class NewShrineEventHandler : EventPacketHandler<NewShrineEvent>
{
    private readonly TrackingController _trackingController;

    public NewShrineEventHandler(TrackingController trackingController) : base((int) EventCodes.NewShrine)
    {
        _trackingController = trackingController;
    }

    protected override async Task OnActionAsync(NewShrineEvent value)
    {
        await _trackingController.DungeonController?.SetDungeonEventInformationAsync(value.Id, value.UniqueName)!;
        await Task.CompletedTask;
    }
}