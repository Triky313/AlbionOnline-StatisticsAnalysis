using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;
using StatisticsAnalysisTool.Network.Events;

namespace StatisticsAnalysisTool.Network.Handler;

public class UpdateMoneyEventHandler : EventPacketHandler<UpdateMoneyEvent>
{
    private readonly LiveStatsTracker _liveStatsTracker;

    public UpdateMoneyEventHandler(TrackingController trackingController) : base((int) EventCodes.UpdateMoney)
    {
        _liveStatsTracker = trackingController?.LiveStatsTracker;
    }

    protected override async Task OnActionAsync(UpdateMoneyEvent value)
    {
        await Task.CompletedTask;
    }
}