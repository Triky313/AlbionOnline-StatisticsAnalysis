using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;
using StatisticsAnalysisTool.Network.Operations.Responses;
using StatisticsAnalysisTool.Enumerations;

namespace StatisticsAnalysisTool.Network.Handler;

public class AuctionGetOffersResponseHandler : ResponsePacketHandler<AuctionGetOffersResponse>
{
    private readonly TrackingController _trackingController;

    public AuctionGetOffersResponseHandler(TrackingController trackingController) : base((int) OperationCodes.AuctionGetOffers)
    {
        _trackingController = trackingController;
    }

    protected override async Task OnActionAsync(AuctionGetOffersResponse value)
    {
        _trackingController.MarketController.AddOffers(value.AuctionEntries);
        await Task.CompletedTask;
    }
}