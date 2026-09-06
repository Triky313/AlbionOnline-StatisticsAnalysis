using StatisticsAnalysisTool.Cluster;
using StatisticsAnalysisTool.EventLogging;
using StatisticsAnalysisTool.EventLogging.Notification;
using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.Models.NetworkModel;
using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
using System;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class DiedEventHandler(TrackingController trackingController) : EventPacketHandler<DiedEvent>((int) EventCodes.Died)
{
    protected override async Task OnActionAsync(DiedEvent value)
    {
        if (trackingController.DungeonController is { } dungeonController)
        {
            await dungeonController.SetDiedIfInDungeonAsync(new DiedObject(value.Died, value.KilledBy, value.KilledByGuild));
        }

        trackingController.PartyController.PlayerHasDied(value.Died);
        trackingController.StatisticController.ResolvePlayerCombatResult(
            value.DiedObjectId,
            value.Died,
            value.KillerObjectId,
            value.KilledBy,
            value.IsLethal);

        if (trackingController.IsKillTrackingEnabled)
        {
            var diedPlayer = trackingController.EntityController.GetEntity(value.DiedObjectId)?.Value
                             ?? trackingController.EntityController.GetEntity(value.Died)?.Value;
            var killerPlayer = trackingController.EntityController.GetEntity(value.KillerObjectId)?.Value
                               ?? trackingController.EntityController.GetEntity(value.KilledBy)?.Value;
            var diedPlayerAlliance = diedPlayer?.Alliance ?? string.Empty;
            var killedByAlliance = killerPlayer?.Alliance ?? string.Empty;
            var clusterName = ClusterController.GetCurrentClusterDisplayName();
            await trackingController.LootController.AddKillDeathAsync(value.Died, value.DiedPlayerGuild, diedPlayerAlliance, value.KilledBy, value.KilledByGuild, killedByAlliance, clusterName);
            await trackingController.AddNotificationAsync(SetKillNotification(value.Died, value.DiedPlayerGuild, diedPlayerAlliance, value.KilledBy, value.KilledByGuild, killedByAlliance, clusterName));
        }
    }

    private static TrackingNotification SetKillNotification(string died, string diedPlayerGuild, string diedPlayerAlliance,
        string killedBy, string killedByGuild, string killedByAlliance, string clusterName)
    {
        var notification = new TrackingNotification(DateTime.Now, new KillNotificationFragment(died, diedPlayerGuild, diedPlayerAlliance,
            killedBy, killedByGuild, killedByAlliance, LocalizationController.Translation("WAS_KILLED_BY")), LoggingFilterType.Kill);
        notification.SetClusterName(clusterName);

        return notification;
    }
}