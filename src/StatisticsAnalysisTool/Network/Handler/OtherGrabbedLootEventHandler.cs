using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class OtherGrabbedLootEventHandler : EventPacketHandler<GrabbedLootEvent>
{
    private readonly TrackingController _trackingController;

    public OtherGrabbedLootEventHandler(TrackingController trackingController) : base((int) EventCodes.GrabbedLoot)
    {
        _trackingController = trackingController;
    }

    protected override async Task OnActionAsync(GrabbedLootEvent value)
    {
        await _trackingController.LootController.AddLootAsync(value.Loot);
    }
}