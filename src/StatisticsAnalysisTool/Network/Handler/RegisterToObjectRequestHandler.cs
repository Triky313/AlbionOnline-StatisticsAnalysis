using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;
using StatisticsAnalysisTool.Network.Operations.Responses;

namespace StatisticsAnalysisTool.Network.Handler;

public class RegisterToObjectRequestHandler
{
    private readonly TrackingController _trackingController;

    public RegisterToObjectRequestHandler(TrackingController trackingController)
    {
        _trackingController = trackingController;
    }

    public async Task OnActionAsync(RegisterToObjectRequest value)
    {
        _trackingController.RegisterBuilding(value.BuildingObjectId);
        await Task.CompletedTask;
    }
}