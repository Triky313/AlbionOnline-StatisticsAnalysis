using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class ActiveSpellEffectsUpdateEventHandler : EventPacketHandler<ActiveSpellEffectsUpdateEvent>
{
    private readonly IEntityController _entityController;

    public ActiveSpellEffectsUpdateEventHandler(IEntityController entityController) : base((int) EventCodes.ActiveSpellEffectsUpdate)
    {
        _entityController = entityController;
    }

    protected override async Task OnActionAsync(ActiveSpellEffectsUpdateEvent value)
    {
        if (value.CauserId != null)
        {
            var spellEffect = new SpellEffect
            {
                CauserId = value.CauserId.ObjectToLong() ?? 0,
                SpellIndex = value.SpellIndex
            };

            _entityController.AddSpellEffect(spellEffect);
        }

        await Task.CompletedTask;
    }
}