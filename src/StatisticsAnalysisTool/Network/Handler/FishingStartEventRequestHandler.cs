using StatisticsAnalysisTool.Gathering;
using StatisticsAnalysisTool.Network.Operations;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class FishingStartEventRequestHandler : RequestPacketHandler<FishingStartRequest>
{
    private readonly IGatheringController _gatheringController;

    public FishingStartEventRequestHandler(IGatheringController gatheringController) : base((int) OperationCodes.FishingStart)
    {
        _gatheringController = gatheringController;
    }

    protected override async Task OnActionAsync(FishingStartRequest value)
    {
        _gatheringController.FishingIsStarted(value.EventId, value.ItemIndex);
        await Task.CompletedTask;
    }
}