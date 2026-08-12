using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
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
            var objectId = (long) value.ObjectId;
            var sourceType = MobController.IsMob(value.LootBody) ? DungeonLootSourceType.Mob : DungeonLootSourceType.Player;
            trackingController.LootController.SetIdentifiedBody(objectId, value.LootBody);
            trackingController.DungeonController.SetLootSource(objectId, value.LootBody, sourceType);
        }

        await Task.CompletedTask;
    }
}