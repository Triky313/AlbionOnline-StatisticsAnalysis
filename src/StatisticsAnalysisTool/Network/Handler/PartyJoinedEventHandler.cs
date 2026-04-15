using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class PartyJoinedEventHandler(TrackingController trackingController) : EventPacketHandler<PartyJoinedEvent>((int) EventCodes.PartyJoined)
{
    protected override async Task OnActionAsync(PartyJoinedEvent value)
    {
        await trackingController.EntityController.SetPartyAsync(value.PartyUsers);
    }
}