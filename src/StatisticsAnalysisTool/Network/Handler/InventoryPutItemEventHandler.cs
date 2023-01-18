using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class InventoryPutItemEventHandler : EventPacketHandler<InventoryPutItemEvent>
{
    private readonly TrackingController _trackingController;

    public InventoryPutItemEventHandler(TrackingController trackingController) : base((int) EventCodes.InventoryPutItem)
    {
        _trackingController = trackingController;
    }

    protected override async Task OnActionAsync(InventoryPutItemEvent value)
    {
        await Task.CompletedTask;
    }
}