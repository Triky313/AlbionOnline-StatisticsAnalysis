using StatisticsAnalysisTool.Gathering;
using StatisticsAnalysisTool.Network.Events;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class HarvestFinishedEventHandler : EventPacketHandler<HarvestFinishedEvent>
{
    private readonly IGatheringController _gatheringController;

    public HarvestFinishedEventHandler(IGatheringController gatheringController) : base((int) EventCodes.HarvestFinished)
    {
        _gatheringController = gatheringController;
    }

    protected override async Task OnActionAsync(HarvestFinishedEvent value)
    {
        await _gatheringController.AddOrUpdateAsync(value.HarvestFinishedObject);
    }
}