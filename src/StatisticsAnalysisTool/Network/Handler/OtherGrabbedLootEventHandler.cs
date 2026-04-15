using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class OtherGrabbedLootEventHandler(TrackingController trackingController) : EventPacketHandler<GrabbedLootEvent>((int) EventCodes.OtherGrabbedLoot)
{
    protected override async Task OnActionAsync(GrabbedLootEvent value)
    {
        await trackingController.LootController.AddLootedItemAsync(value.Loot);
        await trackingController.LootController.AddLootAsync(value.Loot);
    }
}