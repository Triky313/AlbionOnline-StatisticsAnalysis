using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class NewLootEventHandler : EventPacketHandler<NewLootEvent>
{
    private readonly TrackingController _trackingController;

    public NewLootEventHandler(TrackingController trackingController) : base((int) EventCodes.NewLoot)
    {
        _trackingController = trackingController;
    }

    protected override async Task OnActionAsync(NewLootEvent value)
    {


        if (value?.ObjectId != null)
        {
            _trackingController.LootController.SetIdentifiedBody((long) value.ObjectId, value.LootBody);
        }
        await Task.CompletedTask;
    }
}