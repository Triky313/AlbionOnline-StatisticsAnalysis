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


        await _trackingController.EntityController.SetPartyAsync(value.PartyUsers);
    }
}