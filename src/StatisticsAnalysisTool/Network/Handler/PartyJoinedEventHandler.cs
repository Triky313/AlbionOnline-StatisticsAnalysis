using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class PartyJoinedEventHandler : EventPacketHandler<PartyJoinedEvent>
{
    private readonly TrackingController _trackingController;

    public PartyJoinedEventHandler(TrackingController trackingController) : base((int) EventCodes.PartyJoined)
    {
        _trackingController = trackingController;
    }

    protected override async Task OnActionAsync(PartyJoinedEvent value)
    {
        if (!_trackingController.EntityController.IsLocalEntity(value.UserGuid) && !_trackingController.EntityController.ExistEntity(value.UserGuid))
        {
            _trackingController.EntityController
                .AddEntity(null, value.UserGuid, null, value.Username, value.GuildName, null, null, GameObjectType.Player, GameObjectSubType.Player);
        }

        await Task.CompletedTask;
    }
}