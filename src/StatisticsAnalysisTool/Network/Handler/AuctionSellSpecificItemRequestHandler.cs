using StatisticsAnalysisTool.Network.Operations.Responses;
using StatisticsAnalysisTool.Trade.Market;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class AuctionSellSpecificItemRequestHandler : RequestPacketHandler<AuctionSellSpecificItemRequest>
{
    private readonly IMarketController _marketController;

    public AuctionSellSpecificItemRequestHandler(IMarketController marketController) : base((int) OperationCodes.AuctionSellSpecificItemRequest)
    {
        _marketController = marketController;
    }

    protected override async Task OnActionAsync(AuctionSellSpecificItemRequest value)
    {
        await _marketController.AddSaleAsync(value.Sale);
    }
}