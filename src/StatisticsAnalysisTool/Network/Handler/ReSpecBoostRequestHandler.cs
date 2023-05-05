using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Network.Operations.Responses;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class ReSpecBoostRequestHandler : RequestPacketHandler<ReSpecBoostRequest>
{
    private readonly TrackingController _trackingController;

    public ReSpecBoostRequestHandler(TrackingController trackingController) : base((int) OperationCodes.ReSpecBoost)
    {
        _trackingController = trackingController;
    }

    protected override async Task OnActionAsync(ReSpecBoostRequest value)
    {
        _trackingController.EntityController.LocalUserData.IsReSpecActive = value.IsReSpecBoostActive;
        await Task.CompletedTask;
    }
}