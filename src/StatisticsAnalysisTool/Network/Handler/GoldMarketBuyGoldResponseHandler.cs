using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Network.Operations.Responses;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public sealed class GoldMarketBuyGoldResponseHandler(TrackingController trackingController) : ResponsePacketHandler<GoldMarketTradeResponse>((int) OperationCodes.GoldMarketBuyGold)
{
    protected override async Task OnActionAsync(GoldMarketTradeResponse value)
    {
        await trackingController.MarketController.AddGoldBuyAsync(value.Trade);
    }
}