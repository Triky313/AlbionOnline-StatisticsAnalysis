using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class HealthUpdateEventHandler(TrackingController trackingController) : EventPacketHandler<HealthUpdateEvent>((int) EventCodes.HealthUpdate)
{
    protected override async Task OnActionAsync(HealthUpdateEvent value)
    {
        var mob = trackingController.CombatController.CombatEventTracker.GetKnownMobOrDefault(value.AffectedObjectId);
        trackingController.MobKillController.TrackLocalPlayerMobDamage(value.AffectedObjectId, value.CauserId, value.HealthChange);

        if (value.HealthChange < 0 && !value.HasNewHealthValue)
        {
            trackingController.MobKillController.TryAddMobKill(value.AffectedObjectId, mob, value.HealthChange, value.HasNewHealthValue);
        }

        await trackingController.CombatController.AddDamage(value.AffectedObjectId, value.CauserId, value.HealthChange, value.NewHealthValue, value.CausingSpellIndex, value.EffectType);
        await trackingController.CombatController.AddTakenDamage(value.AffectedObjectId, value.CauserId, value.HealthChange, value.NewHealthValue, value.CausingSpellIndex);
    }
}