using StatisticsAnalysisTool.Network.Manager;
using System;
using System.Threading.Tasks;
using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Enumerations;

namespace StatisticsAnalysisTool.Network.Handler;

public class PartyPlayerLeftEventHandler : EventPacketHandler<PartyPlayerLeftEvent>
{
    private readonly TrackingController _trackingController;

    public PartyPlayerLeftEventHandler(TrackingController trackingController) : base((int) EventCodes.PartyPlayerLeft)
    {
        _trackingController = trackingController;
    }

    protected override async Task OnActionAsync(PartyPlayerLeftEvent value)
    {
        await _trackingController.EntityController.RemoveFromPartyAsync(value.UserGuid);
    }
}