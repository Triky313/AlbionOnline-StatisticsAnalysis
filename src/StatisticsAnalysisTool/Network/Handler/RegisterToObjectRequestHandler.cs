using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Network.Operations.Responses;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class RegisterToObjectRequestHandler : RequestPacketHandler<RegisterToObjectRequest>
{
    private readonly TrackingController _trackingController;

    public RegisterToObjectRequestHandler(TrackingController trackingController) : base((int) OperationCodes.RegisterToObject)
    {
        _trackingController = trackingController;
    }

    protected override async Task OnActionAsync(RegisterToObjectRequest value)
    {
        _trackingController.RegisterBuilding(value.BuildingObjectId);
        _trackingController.TradeController.RegisterBuilding(value.BuildingObjectId);
        await Task.CompletedTask;
    }
}