using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;
using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Enumerations;

namespace StatisticsAnalysisTool.Network.Handler;

public class InCombatStateUpdateEventHandler : EventPacketHandler<InCombatStateUpdateEvent>
{
    private readonly TrackingController _trackingController;

    public InCombatStateUpdateEventHandler(TrackingController trackingController) : base((int) EventCodes.InCombatStateUpdate)
    {
        _trackingController = trackingController;
    }

    protected override async Task OnActionAsync(InCombatStateUpdateEvent value)
    {
        if (value.ObjectId != null)
        {
            _trackingController.CombatController.UpdateCombatMode((long)value.ObjectId, value.InActiveCombat, value.InPassiveCombat);
        }

        await Task.CompletedTask;
    }
}