using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Network.Operations;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class FishingFinishResponseHandler : ResponsePacketHandler<FishingFinishResponse>
{
    public FishingFinishResponseHandler() : base((int) OperationCodes.FishingFinish)
    {
    }

    protected override Task OnActionAsync(FishingFinishResponse value)
    {
        return Task.CompletedTask;
    }
}