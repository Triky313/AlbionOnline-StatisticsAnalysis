using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Network.Operations;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class FishingFinishRequestHandler : RequestPacketHandler<FishingFinishRequest>
{
    private readonly TrackingController _trackingController;

    public FishingFinishRequestHandler(TrackingController trackingController) : base((int) OperationCodes.FishingFinish)
    {
        _trackingController = trackingController;
    }

    protected override async Task OnActionAsync(FishingFinishRequest value)
    {
        _trackingController.GatheringController.IsCurrentFishingSucceeded(value.Succeeded);
        await Task.CompletedTask;
    }
}