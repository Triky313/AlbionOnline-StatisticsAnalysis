using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Network.Operations.Responses;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class ActionOnBuildingStartRequestHandler : RequestPacketHandler<ActionOnBuildingStartRequest>
{
    private readonly TrackingController _trackingController;

    public ActionOnBuildingStartRequestHandler(TrackingController trackingController) : base((int) OperationCodes.ActionOnBuildingStart)
    {
        _trackingController = trackingController;
    }

    protected override async Task OnActionAsync(ActionOnBuildingStartRequest value)
    {
        _trackingController.SetUpcomingRepair(value.BuildingObjectId, value.Costs);
        await Task.CompletedTask;
    }
}