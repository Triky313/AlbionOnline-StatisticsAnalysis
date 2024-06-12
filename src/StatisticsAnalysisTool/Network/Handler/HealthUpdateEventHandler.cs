using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;
using StatisticsAnalysisTool.Network.Events;

namespace StatisticsAnalysisTool.Network.Handler;

public class HealthUpdateEventHandler : EventPacketHandler<HealthUpdateEvent>
{
    private readonly TrackingController _trackingController;

    public HealthUpdateEventHandler(TrackingController trackingController) : base((int) EventCodes.HealthUpdate)
    {
        _trackingController = trackingController;
    }

    protected override async Task OnActionAsync(HealthUpdateEvent value)
    {
        await _trackingController.CombatController.AddDamage(value.AffectedObjectId, value.CauserId, value.HealthChange, value.NewHealthValue, value.CausingSpellIndex);
    }
}