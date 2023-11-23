using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Network.Operations.Responses;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class UnRegisterFromObjectRequestHandler : RequestPacketHandler<UnRegisterFromObjectRequest>
{
    private readonly IGameEventWrapper _gameEventWrapper;

    public UnRegisterFromObjectRequestHandler(IGameEventWrapper gameEventWrapper) : base((int) OperationCodes.UnRegisterFromObject)
    {
        _gameEventWrapper = gameEventWrapper;
    }

    protected override async Task OnActionAsync(UnRegisterFromObjectRequest value)
    {
        _gameEventWrapper.TrackingController.UnregisterBuilding(value.BuildingObjectId);
        _gameEventWrapper.TradeController.UnregisterBuilding(value.BuildingObjectId);

        _gameEventWrapper.MarketController.ResetTempOffers();
        _gameEventWrapper.MarketController.ResetTempBuyOrders();
        _gameEventWrapper.MarketController.ResetTempNumberToBuyList();
        await Task.CompletedTask;
    }
}