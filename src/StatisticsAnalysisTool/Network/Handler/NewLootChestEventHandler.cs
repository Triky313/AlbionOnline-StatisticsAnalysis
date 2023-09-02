using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;
using StatisticsAnalysisTool.Network.Events;

namespace StatisticsAnalysisTool.Network.Handler;

public class NewLootChestEventHandler : EventPacketHandler<NewLootChestEvent>
{
    private readonly TrackingController _trackingController;

    public NewLootChestEventHandler(TrackingController trackingController) : base((int) EventCodes.NewLootChest)
    {
        _trackingController = trackingController;
    }

    protected override async Task OnActionAsync(NewLootChestEvent value)
    {
        await _trackingController?.DungeonController?.SetDungeonEventInformationAsync(value.ObjectId, value.UniqueName)!;
        _trackingController?.TreasureController?.AddTreasure(value.ObjectId, value.UniqueName, value.UniqueNameWithLocation);
    }
}