using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class LeaveEventHandler(TrackingController trackingController) : EventPacketHandler<LeaveEvent>((int) EventCodes.Leave)
{
    protected override Task OnActionAsync(LeaveEvent value)
    {
        if (value.ObjectId is { } objectId)
        {
            trackingController.CombatController.CombatEventTracker.RemoveKnownMob(objectId);
        }

        return Task.CompletedTask;
    }
}