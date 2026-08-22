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
            var clusterName = ClusterController.GetCurrentClusterDisplayName();
            await trackingController.LootController.AddKillDeathAsync(value.Died, value.DiedPlayerGuild, value.KilledBy, value.KilledByGuild, clusterName);
            await trackingController.AddNotificationAsync(SetKillNotification(value.Died, value.DiedPlayerGuild, value.KilledBy, value.KilledByGuild, clusterName));
        }
    }

    private static TrackingNotification SetKillNotification(string died, string diedPlayerGuild, string killedBy, string killedByGuild, string clusterName)
    {
        var notification = new TrackingNotification(DateTime.Now, new KillNotificationFragment(died, diedPlayerGuild, killedBy, killedByGuild, LocalizationController.Translation("WAS_KILLED_BY")), LoggingFilterType.Kill);
        notification.SetClusterName(clusterName);

        return notification;
    }
}