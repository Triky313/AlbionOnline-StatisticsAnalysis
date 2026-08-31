using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public sealed class ItemRerollQualityFinishedEventHandler(TrackingController trackingController)
    : EventPacketHandler<ItemRerollQualityFinishedEvent>((int) EventCodes.ItemRerollQualityFinished)
{
    protected override async Task OnActionAsync(ItemRerollQualityFinishedEvent value)
    {
        trackingController.QualityRerollFinished(
            value.ResultItemObjectIds,
            value.SourceItemObjectIds);
        await Task.CompletedTask;
    }
}