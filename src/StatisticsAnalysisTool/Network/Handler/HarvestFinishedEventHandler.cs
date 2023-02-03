using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class HarvestFinishedEventHandler : EventPacketHandler<HarvestFinishedEvent>
{
    private readonly TrackingController _trackingController;

    public HarvestFinishedEventHandler(TrackingController trackingController) : base((int) EventCodes.HarvestFinished)
    {
        _trackingController = trackingController;
    }

    protected override async Task OnActionAsync(HarvestFinishedEvent value)
    {
        await _trackingController.GatheringController.AddOrUpdateAsync(value.HarvestFinishedObject);
    }
}