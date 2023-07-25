using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Network.Operations;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class FishingCancelRequestHandler : RequestPacketHandler<FishingCancelRequest>
{
    private readonly TrackingController _trackingController;

    public FishingCancelRequestHandler(TrackingController trackingController) : base((int) OperationCodes.FishingCancel)
    {
        _trackingController = trackingController;
    }

    protected override async Task OnActionAsync(FishingCancelRequest value)
    {
        await _trackingController.GatheringController.FishingFinishedAsync();
    }
}