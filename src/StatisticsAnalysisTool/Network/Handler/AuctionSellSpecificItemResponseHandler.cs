using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;
using StatisticsAnalysisTool.Network.Operations.Responses;

namespace StatisticsAnalysisTool.Network.Handler;

public class AuctionSellSpecificItemResponseHandler : ResponsePacketHandler<AuctionSellSpecificItemResponse>
{
    private readonly TrackingController _trackingController;

    public AuctionSellSpecificItemResponseHandler(TrackingController trackingController) : base((int) OperationCodes.AuctionSellSpecificItemRequest)
    {
        _trackingController = trackingController;
    }

    protected override async Task OnActionAsync(AuctionSellSpecificItemResponse value)
    {
        await _trackingController.MarketController.AddSaleAsync(value.Sale);
    }
}