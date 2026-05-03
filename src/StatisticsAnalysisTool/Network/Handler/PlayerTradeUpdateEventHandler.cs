using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public sealed class PlayerTradeUpdateEventHandler(TrackingController trackingController) : EventPacketHandler<PlayerTradeUpdateEvent>((int) EventCodes.PlayerTradeUpdate)
{
    protected override async Task OnActionAsync(PlayerTradeUpdateEvent value)
    {
        trackingController.TradeController.UpdatePlayerTrade(value.Update);
        await Task.CompletedTask;
    }
}