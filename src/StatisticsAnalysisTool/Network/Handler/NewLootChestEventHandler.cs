using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;
using StatisticsAnalysisTool.Network.Events;

namespace StatisticsAnalysisTool.Network.Handler;

public class NewLootChestEventHandler
{
    private readonly TrackingController _trackingController;

    public NewLootChestEventHandler(TrackingController trackingController)
    {
        _trackingController = trackingController;
    }

    public async Task OnActionAsync(NewLootChestEvent value)
    {
        _trackingController?.DungeonController?.SetDungeonEventObjectInformationAsync(value.ObjectId, value.UniqueName).ConfigureAwait(false);
        _trackingController?.TreasureController?.AddTreasure(value.ObjectId, value.UniqueName, value.UniqueNameWithLocation);
        await Task.CompletedTask;
    }
}