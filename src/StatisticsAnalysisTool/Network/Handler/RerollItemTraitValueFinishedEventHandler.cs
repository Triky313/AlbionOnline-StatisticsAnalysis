using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public sealed class RerollItemTraitValueFinishedEventHandler(TrackingController trackingController)
    : EventPacketHandler<RerollItemTraitValueFinishedEvent>((int) EventCodes.RerollItemTraitValueFinished)
{
    protected override Task OnActionAsync(RerollItemTraitValueFinishedEvent value)
    {
        trackingController.RerollItemTraitValueFinished(
            value.UserObjectId,
            value.BuildingObjectId,
            value.IsProc);
        return Task.CompletedTask;
    }
}