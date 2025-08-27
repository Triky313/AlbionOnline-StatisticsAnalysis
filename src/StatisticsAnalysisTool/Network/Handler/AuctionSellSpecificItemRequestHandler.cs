using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Network.Operations.Responses;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class AuctionSellSpecificItemRequestHandler : RequestPacketHandler<AuctionSellSpecificItemRequest>
{
    private readonly TrackingController _trackingController;

    public AuctionSellSpecificItemRequestHandler(TrackingController trackingController) : base((int) OperationCodes.AuctionSellSpecificItemRequest)
    {
        _trackingController = trackingController;
    }

    protected override async Task OnActionAsync(AuctionSellSpecificItemRequest value)
    {
        await _trackingController.MarketController.AddSaleAsync(value.Sale);
    }
}