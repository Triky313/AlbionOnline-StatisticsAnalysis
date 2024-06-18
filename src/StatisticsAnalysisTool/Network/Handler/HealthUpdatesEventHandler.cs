using StatisticsAnalysisTool.Models.NetworkModel;
using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class HealthUpdatesEventHandler : EventPacketHandler<HealthUpdatesEvent>
{
    private readonly TrackingController _trackingController;

    public HealthUpdatesEventHandler(TrackingController trackingController) : base((int) EventCodes.HealthUpdates)
    {
        _trackingController = trackingController;
    }

    protected override async Task OnActionAsync(HealthUpdatesEvent value)
    {
        foreach (HealthUpdate healthUpdate in value.HealthUpdates)
        {
            await _trackingController.CombatController.AddDamage(healthUpdate.AffectedObjectId, healthUpdate.CauserId, healthUpdate.HealthChange, healthUpdate.NewHealthValue, healthUpdate.CausingSpellIndex);
        }
    }
}