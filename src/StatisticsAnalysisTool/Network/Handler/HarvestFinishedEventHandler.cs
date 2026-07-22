using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class HarvestFinishedEventHandler(TrackingController trackingController) : EventPacketHandler<HarvestFinishedEvent>((int) EventCodes.HarvestFinished)
{
    protected override Task OnActionAsync(HarvestFinishedEvent value)
    {
        return trackingController.GatheringController.AddOrUpdateAsync(value.HarvestFinishedObject);
    }
}