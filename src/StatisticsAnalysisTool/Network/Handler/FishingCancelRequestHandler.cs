using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Network.Operations;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class FishingCancelRequestHandler(TrackingController trackingController) : RequestPacketHandler<FishingCancelRequest>((int) OperationCodes.FishingCancel)
{
    protected override async Task OnActionAsync(FishingCancelRequest value)
    {
        await trackingController.GatheringController.FishingFinishedAsync();
    }
}