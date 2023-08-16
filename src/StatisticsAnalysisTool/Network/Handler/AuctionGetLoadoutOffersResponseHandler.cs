using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Network.Operations.Responses;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class AuctionGetLoadoutOffersResponseHandler : ResponsePacketHandler<AuctionGetLoadoutOffersResponse>
{
    private readonly TrackingController _trackingController;

    public AuctionGetLoadoutOffersResponseHandler(TrackingController trackingController) : base((int) OperationCodes.AuctionGetLoadoutOffers)
    {
        _trackingController = trackingController;
    }

    protected override async Task OnActionAsync(AuctionGetLoadoutOffersResponse value)
    {
        _trackingController.MarketController.AddOffers(value.AuctionEntries, value.NumberToBuyList);
        await Task.CompletedTask;
    }
}