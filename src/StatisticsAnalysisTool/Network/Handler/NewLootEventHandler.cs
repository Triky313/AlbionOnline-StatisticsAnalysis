using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;
using StatisticsAnalysisTool.Network.Events;

namespace StatisticsAnalysisTool.Network.Handler;

public class NewLootEventHandler
{
    private readonly TrackingController _trackingController;

    public NewLootEventHandler(TrackingController trackingController)
    {
        _trackingController = trackingController;
    }

    public async Task OnActionAsync(NewLootEvent value)
    {
        if (value?.ObjectId != null)
        {
            _trackingController.LootController.SetIdentifiedBody((long)value.ObjectId, value.LootBody);
        }
        await Task.CompletedTask;
    }
}