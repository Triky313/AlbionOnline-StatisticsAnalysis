using StatisticsAnalysisTool.Gathering;
using StatisticsAnalysisTool.Network.Operations;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class FishingCancelRequestHandler : RequestPacketHandler<FishingCancelRequest>
{
    private readonly IGatheringController _gatheringController;

    public FishingCancelRequestHandler(IGatheringController gatheringController) : base((int) OperationCodes.FishingCancel)
    {
        _gatheringController = gatheringController;
    }

    protected override async Task OnActionAsync(FishingCancelRequest value)
    {
        await _gatheringController.FishingFinishedAsync();
    }
}