using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public sealed class InvitationPlayerTradeEventHandler(TrackingController trackingController) : EventPacketHandler<InvitationPlayerTradeEvent>((int) EventCodes.InvitationPlayerTrade)
{
    protected override async Task OnActionAsync(InvitationPlayerTradeEvent value)
    {
        trackingController.TradeController.RegisterPlayerTradeSession(value.TradeId, value.PartnerName);
        await Task.CompletedTask;
    }
}