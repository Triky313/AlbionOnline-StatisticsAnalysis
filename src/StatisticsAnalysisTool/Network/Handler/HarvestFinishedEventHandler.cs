using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class HarvestFinishedEventHandler(TrackingController trackingController) : EventPacketHandler<HarvestFinishedEvent>((int) EventCodes.HarvestFinished)
{
    protected override async Task OnActionAsync(HarvestFinishedEvent value)
    {
        await trackingController.GatheringController.AddOrUpdateAsync(value.HarvestFinishedObject);
    }
}