using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class NewMobEventHandler : EventPacketHandler<NewMobEvent>
{
    private readonly TrackingController _trackingController;

    public NewMobEventHandler(TrackingController trackingController) : base((int) EventCodes.NewMob)
    {
        _trackingController = trackingController;
    }

    protected override async Task OnActionAsync(NewMobEvent value)
    {
        _trackingController.DungeonController.AddTierToCurrentDungeon(value.MobIndex);
        _trackingController.DungeonController.AddLevelToCurrentDungeon(value.MobIndex, value.HitPointsMax);
        await Task.CompletedTask;
    }
}