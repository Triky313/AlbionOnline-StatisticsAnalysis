using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class UpdateSilverEventHandler
{
    private readonly LiveStatsTracker _liveStatsTracker;

    public UpdateSilverEventHandler(TrackingController trackingController)
    {
        _liveStatsTracker = trackingController?.LiveStatsTracker;
    }

    public async Task OnActionAsync(UpdateSilverEvent value)
    {
        await Task.CompletedTask;
    }
}