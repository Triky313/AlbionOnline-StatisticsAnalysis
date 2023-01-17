using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Network.Events;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class InventoryDeleteItemEventHandler
{
    private readonly TrackingController _trackingController;

    public InventoryDeleteItemEventHandler(TrackingController trackingController)
    {
        _trackingController = trackingController;
    }

    public async Task OnActionAsync(InventoryDeleteItemEvent value)
    {
        await Task.CompletedTask;
    }
}