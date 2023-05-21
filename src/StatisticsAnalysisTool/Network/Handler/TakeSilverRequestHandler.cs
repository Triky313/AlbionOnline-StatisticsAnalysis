using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Network.Operations.Responses;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class TakeSilverRequestHandler : RequestPacketHandler<TakeSilverRequest>
{
    private readonly TrackingController _trackingController;

    public TakeSilverRequestHandler(TrackingController trackingController) : base((int) OperationCodes.TakeSilver)
    {
        _trackingController = trackingController;
    }

    protected override async Task OnActionAsync(TakeSilverRequest value)
    {
        _trackingController.SetUpcomingRepair(value.BuildingObjectId, value.Costs);
        await Task.CompletedTask;
    }
}