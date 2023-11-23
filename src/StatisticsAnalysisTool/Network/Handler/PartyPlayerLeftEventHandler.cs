using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class PartyPlayerLeftEventHandler : EventPacketHandler<PartyPlayerLeftEvent>
{
    private readonly IEntityController _entityController;

    public PartyPlayerLeftEventHandler(IEntityController entityController) : base((int) EventCodes.PartyPlayerLeft)
    {
        _entityController = entityController;
    }

    protected override async Task OnActionAsync(PartyPlayerLeftEvent value)
    {
        await _entityController.RemoveFromPartyAsync(value.UserGuid);
    }
}