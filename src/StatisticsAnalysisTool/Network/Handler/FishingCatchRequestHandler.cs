using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Network.Operations;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class FishingCatchRequestHandler(TrackingController trackingController) : RequestPacketHandler<FishingCatchRequest>((int) OperationCodes.FishingCatch)
{
    protected override Task OnActionAsync(FishingCatchRequest value)
    {
        trackingController.GatheringController.FishingCatchStarted(value.ActionId);
        return Task.CompletedTask;
    }
}