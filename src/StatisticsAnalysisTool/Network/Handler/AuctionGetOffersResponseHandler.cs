using StatisticsAnalysisTool.Network.Operations.Responses;
using StatisticsAnalysisTool.Trade.Market;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class AuctionGetOffersResponseHandler : ResponsePacketHandler<AuctionGetOffersResponse>
{
    private readonly IMarketController _marketController;

    public AuctionGetOffersResponseHandler(IMarketController marketController) : base((int) OperationCodes.AuctionGetOffers)
    {
        _marketController = marketController;
    }

    protected override async Task OnActionAsync(AuctionGetOffersResponse value)
    {
        _marketController.AddOffers(value.AuctionEntries);
        await Task.CompletedTask;
    }
}