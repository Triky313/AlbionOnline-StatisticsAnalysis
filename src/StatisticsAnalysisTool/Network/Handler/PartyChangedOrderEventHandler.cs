using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;
using StatisticsAnalysisTool.Network.Events;

namespace StatisticsAnalysisTool.Network.Handler;

public class PartyChangedOrderEventHandler : EventPacketHandler<PartyChangedOrderEvent>
{
    private readonly TrackingController _trackingController;

    public PartyChangedOrderEventHandler(TrackingController trackingController) : base((int) EventCodes.PartyChangedOrder)
    {
        _trackingController = trackingController;
    }

    protected override async Task OnActionAsync(PartyChangedOrderEvent value)
    {
        await _trackingController.EntityController.SetPartyAsync(value.PartyUsers, true);
    }
}