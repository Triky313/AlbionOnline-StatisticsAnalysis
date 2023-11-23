using StatisticsAnalysisTool.Gathering;
using StatisticsAnalysisTool.Network.Operations;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class FishingFinishRequestHandler : RequestPacketHandler<FishingFinishRequest>
{
    private readonly IGatheringController _gatheringController;

    public FishingFinishRequestHandler(IGatheringController gatheringController) : base((int) OperationCodes.FishingFinish)
    {
        _gatheringController = gatheringController;
    }

    protected override async Task OnActionAsync(FishingFinishRequest value)
    {
        _gatheringController.IsCurrentFishingSucceeded(value.Succeeded);
        await Task.CompletedTask;
    }
}