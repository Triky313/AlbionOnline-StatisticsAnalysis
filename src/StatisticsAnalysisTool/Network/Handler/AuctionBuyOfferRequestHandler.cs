using StatisticsAnalysisTool.Network.Operations.Responses;
using StatisticsAnalysisTool.Trade.Market;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class AuctionBuyOfferRequestHandler : RequestPacketHandler<AuctionBuyOfferRequest>
{
    private readonly IMarketController _marketController;

    public AuctionBuyOfferRequestHandler(IMarketController marketController) : base((int) OperationCodes.AuctionBuyOffer)
    {
        _marketController = marketController;
    }

    protected override async Task OnActionAsync(AuctionBuyOfferRequest value)
    {
        await _marketController.AddBuyAsync(value.Purchase);
    }
}