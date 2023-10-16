using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;

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
        await Task.CompletedTask;
    }
}