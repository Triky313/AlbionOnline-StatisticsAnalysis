using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;
using StatisticsAnalysisTool.Network.Events;

namespace StatisticsAnalysisTool.Network.Handler;

public class UpdateSilverEventHandler : EventPacketHandler<UpdateSilverEvent>
{
    private readonly LiveStatsTracker _liveStatsTracker;

    public UpdateSilverEventHandler(TrackingController trackingController) : base((int) EventCodes.UpdateSilver)
    {
        _liveStatsTracker = trackingController?.LiveStatsTracker;
    }

    protected override async Task OnActionAsync(UpdateSilverEvent value)
    {
        await Task.CompletedTask;
    }
}