using StatisticsAnalysisTool.Gathering;
using StatisticsAnalysisTool.Network.Operations;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class FishingFinishResponseHandler : ResponsePacketHandler<FishingFinishResponse>
{
    private readonly IGatheringController _gatheringController;

    public FishingFinishResponseHandler(IGatheringController gatheringController) : base((int) OperationCodes.FishingFinish)
    {
        _gatheringController = gatheringController;
    }

    protected override async Task OnActionAsync(FishingFinishResponse value)
    {
        _gatheringController.CloseFishingEvent();
        await Task.CompletedTask;
    }
}