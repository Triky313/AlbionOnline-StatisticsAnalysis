using StatisticsAnalysisTool.Network.Operations.Responses;
using StatisticsAnalysisTool.Trade.Market;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class AuctionGetResponseHandler : ResponsePacketHandler<AuctionGetResponse>
{
    private readonly IMarketController _marketController;

    public AuctionGetResponseHandler(IMarketController marketController) : base((int) OperationCodes.AuctionGetRequests)
    {
        _marketController = marketController;
    }

    protected override async Task OnActionAsync(AuctionGetResponse value)
    {
        _marketController.AddBuyOrders(value.AuctionEntries);
        await Task.CompletedTask;
    }
}