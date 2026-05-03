using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public sealed class PlayerTradeCancelEventHandler(TrackingController trackingController) : EventPacketHandler<PlayerTradeCancelEvent>((int) EventCodes.PlayerTradeCancel)
{
    protected override async Task OnActionAsync(PlayerTradeCancelEvent value)
    {
        trackingController.TradeController.RemovePlayerTradeSession(value.TradeId);
        await Task.CompletedTask;
    }
}