using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;
using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Enumerations;

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
        _trackingController?.DungeonController?.SetDungeonEventObjectInformationAsync(value.ObjectId, value.UniqueName).ConfigureAwait(false);
        _trackingController?.TreasureController?.AddTreasure(value.ObjectId, value.UniqueName, value.UniqueNameWithLocation);
        await Task.CompletedTask;
    }
}