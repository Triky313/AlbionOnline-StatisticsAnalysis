using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Network.Operations.Responses;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public sealed class GoldMarketSellGoldResponseHandler(TrackingController trackingController) : ResponsePacketHandler<GoldMarketTradeResponse>((int) OperationCodes.GoldMarketSellGold)
{
    protected override async Task OnActionAsync(GoldMarketTradeResponse value)
    {
        await trackingController.MarketController.AddGoldSaleAsync(value.Trade);
    }
}