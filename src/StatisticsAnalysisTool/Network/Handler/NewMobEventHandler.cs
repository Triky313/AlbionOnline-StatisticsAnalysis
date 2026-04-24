using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class NewMobEventHandler(TrackingController trackingController) : EventPacketHandler<NewMobEvent>((int) EventCodes.NewMob)
{
    protected override async Task OnActionAsync(NewMobEvent value)
    {
        await trackingController.DungeonController.AddTierToCurrentDungeonAsync(value.MobIndex);
        trackingController.DungeonController.UpdateCurrentDungeonLevel(value.MobIndex, value.HitPointsMax);
    }
}