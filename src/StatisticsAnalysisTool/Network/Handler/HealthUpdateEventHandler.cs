using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class HealthUpdateEventHandler(TrackingController trackingController) : EventPacketHandler<HealthUpdateEvent>((int) EventCodes.HealthUpdate)
{
    protected override async Task OnActionAsync(HealthUpdateEvent value)
    {
        if (value.HealthChange < 0 && !value.HasNewHealthValue)
        {
            await trackingController.OpenWorldController.TryAddMobKillAsync(
                trackingController.CombatController.CombatEventTracker.GetKnownMobOrDefault(value.AffectedObjectId),
                value.HealthChange,
                value.HasNewHealthValue);
        }

        await trackingController.CombatController.AddDamage(value.AffectedObjectId, value.CauserId, value.HealthChange, value.NewHealthValue, value.CausingSpellIndex);
        await trackingController.CombatController.AddTakenDamage(value.AffectedObjectId, value.CauserId, value.HealthChange, value.NewHealthValue, value.CausingSpellIndex);
    }
}