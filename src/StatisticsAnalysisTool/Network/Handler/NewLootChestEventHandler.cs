using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class NewLootChestEventHandler(TrackingController trackingController) : EventPacketHandler<NewLootChestEvent>((int) EventCodes.NewLootChest)
{
    protected override async Task OnActionAsync(NewLootChestEvent value)
    {


        await trackingController?.DungeonController?.SetDungeonEventInformationAsync(value.ObjectId, value.UniqueName)!;
        trackingController?.TreasureController?.AddTreasure(value.ObjectId, value.UniqueName, value.UniqueNameWithLocation);
    }
}