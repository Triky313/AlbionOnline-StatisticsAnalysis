using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Network.Events;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class InventoryDeleteItemEventHandler : EventPacketHandler<InventoryDeleteItemEvent>
{
    private readonly TrackingController _trackingController;

    public InventoryDeleteItemEventHandler(TrackingController trackingController) : base((int) EventCodes.InventoryDeleteItem)
    {
        _trackingController = trackingController;
    }

    protected override async Task OnActionAsync(InventoryDeleteItemEvent value)
    {
        await Task.CompletedTask;
    }
}