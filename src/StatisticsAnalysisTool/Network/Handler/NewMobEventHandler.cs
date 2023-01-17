using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class NewMobEventHandler
{
    private readonly TrackingController _trackingController;

    public NewMobEventHandler(TrackingController trackingController)
    {
        _trackingController = trackingController;
    }

    public async Task OnActionAsync(NewMobEvent value)
    {
        _trackingController.DungeonController.AddTierToCurrentDungeon(value.MobIndex);
        _trackingController.DungeonController.AddLevelToCurrentDungeon(value.MobIndex, value.HitPointsMax);
        await Task.CompletedTask;
    }
}