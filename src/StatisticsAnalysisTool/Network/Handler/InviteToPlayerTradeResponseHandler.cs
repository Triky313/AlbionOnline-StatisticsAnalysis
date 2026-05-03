using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Network.Operations.Responses;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public sealed class InviteToPlayerTradeResponseHandler(TrackingController trackingController) : ResponsePacketHandler<InviteToPlayerTradeResponse>((int) OperationCodes.InviteToPlayerTrade)
{
    protected override async Task OnActionAsync(InviteToPlayerTradeResponse value)
    {
        trackingController.TradeController.RegisterPlayerTradeSession(value.TradeId, value.PartnerName);
        await Task.CompletedTask;
    }
}