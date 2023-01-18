using StatisticsAnalysisTool.Network.Manager;
using System;
using System.Threading.Tasks;
using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Enumerations;

namespace StatisticsAnalysisTool.Network.Handler;

public class PartyPlayerJoinedEventHandler : EventPacketHandler<PartyPlayerJoinedEvent>
{
    private readonly TrackingController _trackingController;

    public PartyPlayerJoinedEventHandler(TrackingController trackingController) : base((int) EventCodes.PartyPlayerJoined)
    {
        _trackingController = trackingController;
    }

    protected override async Task OnActionAsync(PartyPlayerJoinedEvent value)
    {
        if (value?.UserGuid != null)
        {
            await _trackingController.EntityController.AddToPartyAsync((Guid)value.UserGuid, value.Username);
        }
    }
}