using StatisticsAnalysisTool.Network.Operations.Responses;
using StatisticsAnalysisTool.Trade.Market;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class AuctionGetLoadoutOffersResponseHandler : ResponsePacketHandler<AuctionGetLoadoutOffersResponse>
{
    private readonly IMarketController _marketController;

    public AuctionGetLoadoutOffersResponseHandler(IMarketController marketController) : base((int) OperationCodes.AuctionGetLoadoutOffers)
    {
        _marketController = marketController;
    }

    protected override async Task OnActionAsync(AuctionGetLoadoutOffersResponse value)
    {
        _marketController.AddOffers(value.AuctionEntries, value.NumberToBuyList);
        await Task.CompletedTask;
    }
}