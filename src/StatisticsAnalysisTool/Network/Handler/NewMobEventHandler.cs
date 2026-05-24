using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class NewMobEventHandler(TrackingController trackingController) : EventPacketHandler<NewMobEvent>((int) EventCodes.NewMob)
{
    protected override async Task OnActionAsync(NewMobEvent value)
    {
        trackingController.CombatController.CombatEventTracker.TrackNewMob(value);

        if (value.ObjectId is { } pendingMobObjectId)
        {
            var pendingKillRecorded = await trackingController.OpenWorldController.TryAddPendingMobKillAsync(
                pendingMobObjectId,
                trackingController.CombatController.CombatEventTracker.GetKnownMobOrDefault(pendingMobObjectId));

            if (!pendingKillRecorded)
            {
                trackingController.OpenWorldController.ResetRecordedMobKill(pendingMobObjectId);
            }
        }

        await trackingController.DungeonController.AddTierToCurrentDungeonAsync(value.MobIndex);
        trackingController.DungeonController.UpdateCurrentDungeonLevel(value.MobIndex, value.HitPointsMax);
    }
}