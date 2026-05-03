using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public sealed class PlayerTradeFinishedEventHandler(TrackingController trackingController) : EventPacketHandler<PlayerTradeFinishedEvent>((int) EventCodes.PlayerTradeFinished)
{
    protected override async Task OnActionAsync(PlayerTradeFinishedEvent value)
    {
        await trackingController.TradeController.PlayerTradeFinishedAsync(value.TradeId);
    }
}