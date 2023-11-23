using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class PartyJoinedEventHandler : EventPacketHandler<PartyJoinedEvent>
{
    private readonly IEntityController _entityController;

    public PartyJoinedEventHandler(IEntityController entityController) : base((int) EventCodes.PartyJoined)
    {
        _entityController = entityController;
    }

    protected override async Task OnActionAsync(PartyJoinedEvent value)
    {
        await _entityController.SetPartyAsync(value.PartyUsers);
    }
}