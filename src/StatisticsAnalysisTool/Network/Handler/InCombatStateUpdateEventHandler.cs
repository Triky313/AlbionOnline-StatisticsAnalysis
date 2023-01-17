using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;
using StatisticsAnalysisTool.Network.Events;

namespace StatisticsAnalysisTool.Network.Handler;

public class InCombatStateUpdateEventHandler
{
    private readonly TrackingController _trackingController;

    public InCombatStateUpdateEventHandler(TrackingController trackingController)
    {
        _trackingController = trackingController;
    }

    public async Task OnActionAsync(InCombatStateUpdateEvent value)
    {
        if (value.ObjectId != null)
        {
            _trackingController.CombatController.UpdateCombatMode((long)value.ObjectId, value.InActiveCombat, value.InPassiveCombat);
        }

        await Task.CompletedTask;
    }
}