using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;
using StatisticsAnalysisTool.Network.Operations.Responses;

namespace StatisticsAnalysisTool.Network.Handler;

public class RegisterToObjectRequestHandler : RequestPacketHandler<RegisterToObjectRequest>
{
    private readonly IGameEventWrapper _gameEventWrapper;

    public RegisterToObjectRequestHandler(IGameEventWrapper gameEventWrapper) : base((int) OperationCodes.RegisterToObject)
    {
        _gameEventWrapper = gameEventWrapper;
    }

    protected override async Task OnActionAsync(RegisterToObjectRequest value)
    {
        _gameEventWrapper.TrackingController.RegisterBuilding(value.BuildingObjectId);
        _gameEventWrapper.TradeController.RegisterBuilding(value.BuildingObjectId);
        await Task.CompletedTask;
    }
}