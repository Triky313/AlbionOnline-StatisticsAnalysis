using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;
using StatisticsAnalysisTool.Network.Operations.Responses;

namespace StatisticsAnalysisTool.Network.Handler;

public class AuctionGetOffersResponseHandler(TrackingController trackingController) : ResponsePacketHandler<AuctionGetOffersResponse>((int) OperationCodes.AuctionGetOffers)
{
    protected override async Task OnActionAsync(AuctionGetOffersResponse value)
    {
        trackingController.MarketController.AddOffers(value.AuctionEntries);
        trackingController.MarketController.UpdateSellOrderMarketData(value.AuctionEntries);
        await Task.CompletedTask;
    }
}