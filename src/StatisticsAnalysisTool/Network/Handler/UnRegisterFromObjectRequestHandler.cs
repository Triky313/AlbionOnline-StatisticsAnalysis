using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Network.Operations.Responses;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class UnRegisterFromObjectRequestHandler : RequestPacketHandler<UnRegisterFromObjectRequest>
{
    private readonly TrackingController _trackingController;

    public UnRegisterFromObjectRequestHandler(TrackingController trackingController) : base((int) OperationCodes.UnRegisterFromObject)
    {
        _trackingController = trackingController;
    }

    protected override async Task OnActionAsync(UnRegisterFromObjectRequest value)
    {
        _trackingController.UnregisterBuilding(value.BuildingObjectId);
        _trackingController.TradeController.UnregisterBuilding(value.BuildingObjectId);

        _trackingController.MarketController.ResetTempOffers();
        _trackingController.MarketController.ResetTempBuyOrders();
        _trackingController.MarketController.ResetTempNumberToBuyList();
        await Task.CompletedTask;
    }
}