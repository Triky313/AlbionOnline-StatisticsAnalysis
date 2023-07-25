using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Network.Operations;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class FishingStartEventRequestHandler : RequestPacketHandler<FishingStartRequest>
{
    private readonly TrackingController _trackingController;

    public FishingStartEventRequestHandler(TrackingController trackingController) : base((int) OperationCodes.FishingStart)
    {
        _trackingController = trackingController;
    }

    protected override async Task OnActionAsync(FishingStartRequest value)
    {
        await Task.CompletedTask;
    }
}