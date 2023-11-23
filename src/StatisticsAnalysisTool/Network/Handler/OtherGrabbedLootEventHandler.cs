using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class OtherGrabbedLootEventHandler : EventPacketHandler<GrabbedLootEvent>
{
    private readonly ILootController _lootController;

    public OtherGrabbedLootEventHandler(ILootController lootController) : base((int) EventCodes.OtherGrabbedLoot)
    {
        _lootController = lootController;
    }

    protected override async Task OnActionAsync(GrabbedLootEvent value)
    {
        await _lootController.AddLootAsync(value.Loot);
    }
}