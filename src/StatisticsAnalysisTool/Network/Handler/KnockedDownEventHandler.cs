using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public sealed class KnockedDownEventHandler(TrackingController trackingController)
    : EventPacketHandler<KnockedDownEvent>((int) EventCodes.KnockedDown)
{
    protected override Task OnActionAsync(KnockedDownEvent value)
    {
        trackingController.StatisticController.ResolvePlayerCombatResult(
            value.PlayerObjectId,
            value.PlayerName,
            value.KnockedDownByObjectId,
            value.KnockedDownByName,
            false);
        return Task.CompletedTask;
    }
}