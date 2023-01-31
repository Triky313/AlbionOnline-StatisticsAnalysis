using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;
using StatisticsAnalysisTool.Network.Operations.Responses;

namespace StatisticsAnalysisTool.Network.Handler;

public class AuctionGetResponseHandler : ResponsePacketHandler<AuctionGetResponse>
{
    private readonly TrackingController _trackingController;

    public AuctionGetResponseHandler(TrackingController trackingController) : base((int) OperationCodes.AuctionGetRequests)
    {
        _trackingController = trackingController;
    }

    protected override async Task OnActionAsync(AuctionGetResponse value)
    {
        _trackingController.MarketController.AddBuyOrders(value.AuctionEntries);
        await Task.CompletedTask;
    }
}