using StatisticsAnalysisTool.Models.NetworkModel;
using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class HealthUpdatesEventHandler(TrackingController trackingController) : EventPacketHandler<HealthUpdatesEvent>((int) EventCodes.HealthUpdates)
{
    protected override async Task OnActionAsync(HealthUpdatesEvent value)
    {
        foreach (HealthUpdate healthUpdate in value.HealthUpdates)
        {
            var mob = trackingController.CombatController.CombatEventTracker.GetKnownMobOrDefault(healthUpdate.AffectedObjectId);
            trackingController.MobKillController.TrackLocalPlayerMobDamage(healthUpdate.AffectedObjectId, healthUpdate.CauserId, healthUpdate.HealthChange);

            if (healthUpdate.HealthChange < 0 && !healthUpdate.HasNewHealthValue)
            {
                trackingController.MobKillController.TryAddMobKill(healthUpdate.AffectedObjectId, mob, healthUpdate.HealthChange, healthUpdate.HasNewHealthValue);
            }

            await trackingController.CombatController.AddDamage(healthUpdate.AffectedObjectId, healthUpdate.CauserId, healthUpdate.HealthChange, healthUpdate.NewHealthValue, healthUpdate.CausingSpellIndex, healthUpdate.EffectType);
            await trackingController.CombatController.AddTakenDamage(healthUpdate.AffectedObjectId, healthUpdate.CauserId, healthUpdate.HealthChange, healthUpdate.NewHealthValue, healthUpdate.CausingSpellIndex);
        }
    }
}