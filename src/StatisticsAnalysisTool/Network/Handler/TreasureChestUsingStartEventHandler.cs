using StatisticsAnalysisTool.Network.Events;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

/// <summary>
///     Triggered when silver is picked up. Each party member gets their own event.
/// </summary>
public class TreasureChestUsingStartEventHandler : EventPacketHandler<TreasureChestUsingStartEvent>
{
    public TreasureChestUsingStartEventHandler() : base((int) EventCodes.TreasureChestUsingStart)
    {
    }

    protected override async Task OnActionAsync(TreasureChestUsingStartEvent value)
    {
        await Task.CompletedTask;
    }
}