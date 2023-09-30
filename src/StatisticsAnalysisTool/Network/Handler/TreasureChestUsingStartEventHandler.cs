using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;
using StatisticsAnalysisTool.Network.Events;

namespace StatisticsAnalysisTool.Network.Handler;

/// <summary>
///     Triggered when silver is picked up. Each party member gets their own event.
/// </summary>
public class TreasureChestUsingStartEventHandler : EventPacketHandler<TreasureChestUsingStartEvent>
{
    private readonly TrackingController _trackingController;

    public TreasureChestUsingStartEventHandler(TrackingController trackingController) : base((int) EventCodes.TreasureChestUsingStart)
    {
        _trackingController = trackingController;
    }

    protected override async Task OnActionAsync(TreasureChestUsingStartEvent value)
    {
        await Task.CompletedTask;
    }
}