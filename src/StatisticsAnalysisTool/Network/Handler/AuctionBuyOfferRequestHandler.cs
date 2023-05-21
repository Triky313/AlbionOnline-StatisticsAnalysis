using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Network.Operations.Responses;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class AuctionBuyOfferRequestHandler : RequestPacketHandler<AuctionBuyOfferRequest>
{
    private readonly TrackingController _trackingController;

    public AuctionBuyOfferRequestHandler(TrackingController trackingController) : base((int) OperationCodes.AuctionBuyOffer)
    {
        _trackingController = trackingController;
    }

    protected override async Task OnActionAsync(AuctionBuyOfferRequest value)
    {
        await _trackingController.MarketController.AddBuyAsync(value.Purchase);
    }
}