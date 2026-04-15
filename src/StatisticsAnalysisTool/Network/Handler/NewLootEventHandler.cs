using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class NewLootEventHandler(TrackingController trackingController) : EventPacketHandler<NewLootEvent>((int) EventCodes.NewLoot)
{
    protected override async Task OnActionAsync(NewLootEvent value)
    {
        if (value?.ObjectId != null)
        {
            trackingController.LootController.SetIdentifiedBody((long) value.ObjectId, value.LootBody);
        }
        await Task.CompletedTask;
    }
}