using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class HealthUpdateEventHandler : EventPacketHandler<HealthUpdateEvent>
{
    private readonly ICombatController _combatController;

    public HealthUpdateEventHandler(ICombatController combatController) : base((int) EventCodes.HealthUpdate)
    {
        _combatController = combatController;
    }

    protected override async Task OnActionAsync(HealthUpdateEvent value)
    {
        await _combatController.AddDamage(value.AffectedObjectId, value.CauserId, value.HealthChange, value.NewHealthValue);
    }
}