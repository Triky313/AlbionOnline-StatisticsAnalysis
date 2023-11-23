using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Models.NetworkModel;
using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class PartyPlayerJoinedEventHandler : EventPacketHandler<PartyPlayerJoinedEvent>
{
    private readonly IEntityController _entityController;

    public PartyPlayerJoinedEventHandler(IEntityController entityController) : base((int) EventCodes.PartyPlayerJoined)
    {
        _entityController = entityController;
    }

    protected override async Task OnActionAsync(PartyPlayerJoinedEvent value)
    {
        _trackingController?.EntityController?.AddEntity(new Entity
        {
            UserGuid = value.UserGuid,
            Name = value.Username,
            ObjectType = GameObjectType.Player,
            ObjectSubType = GameObjectSubType.Mob
        });

        await _trackingController?.EntityController?.AddToPartyAsync(value.UserGuid)!;
    }
}