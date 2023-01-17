using System;
using StatisticsAnalysisTool.Models.NetworkModel;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Network.Events;
using System.Threading.Tasks;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Notification;

namespace StatisticsAnalysisTool.Network.Handler;

public class DiedEventHandler
{
    private readonly TrackingController _trackingController;

    public DiedEventHandler(TrackingController trackingController)
    {
        _trackingController = trackingController;
    }

    public async Task OnActionAsync(DiedEvent value)
    {
        _trackingController.DungeonController?.SetDiedIfInDungeon(new DiedObject(value.Died, value.KilledBy, value.KilledByGuild));
        await _trackingController.AddNotificationAsync(SetKillNotification(value.Died, value.KilledBy, value.KilledByGuild));
    }

    private static TrackingNotification SetKillNotification(string died, string killedBy, string killedByGuild)
    {
        return new TrackingNotification(DateTime.Now, new KillNotificationFragment(died, killedBy, killedByGuild, LanguageController.Translation("WAS_KILLED_BY")), NotificationType.Kill);
    }
}