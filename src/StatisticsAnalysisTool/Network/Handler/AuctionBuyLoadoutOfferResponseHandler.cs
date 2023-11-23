using StatisticsAnalysisTool.Network.Operations.Responses;
using StatisticsAnalysisTool.Trade.Market;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class AuctionBuyLoadoutOfferResponseHandler : ResponsePacketHandler<AuctionBuyLoadoutOfferResponse>
{
    private readonly IMarketController _marketController;

    public AuctionBuyLoadoutOfferResponseHandler(IMarketController marketController) : base((int) OperationCodes.AuctionBuyLoadoutOffer)
    {
        _marketController = marketController;
    }

    protected override async Task OnActionAsync(AuctionBuyLoadoutOfferResponse value)
    {
        await _marketController.AddBuyAsync(value.PurchaseIds);
    }
}