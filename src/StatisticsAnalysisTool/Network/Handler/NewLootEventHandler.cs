using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class NewLootEventHandler : EventPacketHandler<NewLootEvent>
{
    private readonly ILootController _lootController;

    public NewLootEventHandler(ILootController lootController) : base((int) EventCodes.NewLoot)
    {
        _lootController = lootController;
    }

    protected override async Task OnActionAsync(NewLootEvent value)
    {
        if (value?.ObjectId != null)
        {
            _lootController.SetIdentifiedBody((long) value.ObjectId, value.LootBody);
        }
        await Task.CompletedTask;
    }
}