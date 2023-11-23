using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;
using StatisticsAnalysisTool.Network.Events;

namespace StatisticsAnalysisTool.Network.Handler;

public class InCombatStateUpdateEventHandler : EventPacketHandler<InCombatStateUpdateEvent>
{
    private readonly ICombatController _combatController;

    public InCombatStateUpdateEventHandler(ICombatController combatController) : base((int) EventCodes.InCombatStateUpdate)
    {
        _combatController = combatController;
    }

    protected override async Task OnActionAsync(InCombatStateUpdateEvent value)
    {
        if (value.ObjectId != null)
        {
            _combatController.UpdateCombatMode((long)value.ObjectId, value.InActiveCombat, value.InPassiveCombat);
        }

        await Task.CompletedTask;
    }
}