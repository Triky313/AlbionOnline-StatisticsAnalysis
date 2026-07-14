using StatisticsAnalysisTool.Network.Events;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class HarvestFinishedEventHandler() : EventPacketHandler<HarvestFinishedEvent>((int) EventCodes.HarvestFinished)
{
    protected override Task OnActionAsync(HarvestFinishedEvent value)
    {
        return Task.CompletedTask;
    }
}