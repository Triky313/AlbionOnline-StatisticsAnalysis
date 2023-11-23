using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class PartyDisbandedEventHandler : EventPacketHandler<PartyDisbandedEvent>
{
    private readonly IEntityController _entityController;

    public PartyDisbandedEventHandler(IEntityController entityController) : base((int) EventCodes.PartyDisbanded)
    {
        _entityController = entityController;
    }

    protected override async Task OnActionAsync(PartyDisbandedEvent value)
    {
        await _entityController.ResetPartyMemberAsync();
        await _entityController.AddLocalEntityToPartyAsync();
    }
}