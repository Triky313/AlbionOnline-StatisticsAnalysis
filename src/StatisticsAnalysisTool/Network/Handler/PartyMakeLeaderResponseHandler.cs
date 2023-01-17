using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Network.Operations.Responses;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class PartyMakeLeaderResponseHandler
{
    private readonly TrackingController _trackingController;

    public PartyMakeLeaderResponseHandler(TrackingController trackingController)
    {
        _trackingController = trackingController;
    }

    public async Task OnActionAsync(PartyMakeLeaderResponse value)
    {
        await Task.CompletedTask;
    }
}