using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Network.Operations.Responses;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class TakeSilverRequestHandler
{
    private readonly TrackingController _trackingController;

    public TakeSilverRequestHandler(TrackingController trackingController)
    {
        _trackingController = trackingController;
    }

    public async Task OnActionAsync(TakeSilverRequest value)
    {
        _trackingController.SetUpcomingRepair(value.BuildingObjectId, value.Costs);
        await Task.CompletedTask;
    }
}