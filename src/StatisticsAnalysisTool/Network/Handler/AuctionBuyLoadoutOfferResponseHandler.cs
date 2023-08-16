using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Network.Operations.Responses;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class AuctionBuyLoadoutOfferResponseHandler : ResponsePacketHandler<AuctionBuyLoadoutOfferResponse>
{
    private readonly TrackingController _trackingController;

    public AuctionBuyLoadoutOfferResponseHandler(TrackingController trackingController) : base((int) OperationCodes.AuctionBuyLoadoutOffer)
    {
        _trackingController = trackingController;
    }

    protected override async Task OnActionAsync(AuctionBuyLoadoutOfferResponse value)
    {
        await _trackingController.MarketController.AddBuyAsync(value.PurchaseIds);
    }
}