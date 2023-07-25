using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Network.Operations;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class FishingFinishResponseHandler : ResponsePacketHandler<FishingFinishResponse>
{
    private readonly TrackingController _trackingController;

    public FishingFinishResponseHandler(TrackingController trackingController) : base((int) OperationCodes.FishingFinish)
    {
        _trackingController = trackingController;
    }

    protected override async Task OnActionAsync(FishingFinishResponse value)
    {
        await Task.CompletedTask;
    }
}